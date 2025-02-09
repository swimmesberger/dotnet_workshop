using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;


namespace ChatApp.SourceGenerator;

[Generator]
public class GrainIncrementalSourceGenerator : IIncrementalGenerator {

    private static readonly string GrainNamespace = "ChatApp.Common.Grains";
    private static readonly string GrainInterfaceName = "ChatApp.Common.Grains.IGrain";
    private static readonly string GrainIntefaceSuffix = "IGrain";
    private static readonly string FullCancellationTokenType = "System.Threading.CancellationToken";
    private static readonly string CancellationTokenType = "CancellationToken";
    private static readonly string TaskType = "System.Threading.Tasks.Task";

    public void Initialize(IncrementalGeneratorInitializationContext context) {
        // Filter classes implementing IGrain interface
        var provider = context.SyntaxProvider
            .CreateSyntaxProvider(
                FilterForSourceGen,
                GetForSourceGen
            );
        // Generate the source code.
        context.RegisterSourceOutput(context.CompilationProvider.Combine(provider.Collect()),
            (ctx, t) => GenerateCode(ctx, t.Left, t.Right));
    }

    private static bool FilterForSourceGen(SyntaxNode node, CancellationToken cancellationToken = default) {
        if (node is not ClassDeclarationSyntax declarationSyntax) {
            return false;
        }

        if (declarationSyntax.BaseList == null) return false;
        return declarationSyntax.BaseList.Types
            .Select(baseTypeSyntax => baseTypeSyntax.Type)
            .OfType<IdentifierNameSyntax>()
            .Select(identifierNameSyntax => identifierNameSyntax.Identifier.Text)
            .Any(x => x.Equals(GrainIntefaceSuffix));
    }

    private static ClassDeclarationSyntax GetForSourceGen(GeneratorSyntaxContext context, CancellationToken cancellationToken = default) {
        return (ClassDeclarationSyntax)context.Node;
    }

    private static ITypeSymbol? GetReturnType(IMethodSymbol methodSymbol) {
        var returnType = methodSymbol.ReturnType;
        if (returnType is INamedTypeSymbol { IsGenericType: true } namedTypeSymbol) {
            returnType = namedTypeSymbol.TypeArguments.FirstOrDefault();
        }
        if (returnType?.SpecialType == SpecialType.System_Void) {
            returnType = null;
        }

        string? returnTypeName = returnType?.ToDisplayString();
        if (returnTypeName == TaskType) {
            returnType = null;
        }
        return returnType;
    }

    private void GenerateCode(SourceProductionContext context, Compilation compilation,
        ImmutableArray<ClassDeclarationSyntax> grainTypes) {
        // Go through all filtered class declarations.
        foreach (var grainType in grainTypes) {
            // We need to get semantic model of the class to retrieve metadata.
            var semanticModel = compilation.GetSemanticModel(grainType.SyntaxTree);

            // Symbols allow us to get the compile-time information.
            if (semanticModel.GetDeclaredSymbol(grainType) is not INamedTypeSymbol classSymbol)
                continue;

            if (!classSymbol.AllInterfaces.Any(i => i.ToDisplayString() == GrainInterfaceName))
                continue;

            var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();

            // 'Identifier' means the token of the node. Get class name from the syntax node.
            var originalClassName = grainType.Identifier.Text;
            var className = originalClassName;
            if (originalClassName.EndsWith("Grain")) {
                className = originalClassName.Substring(0, originalClassName.Length - 5);
            }

            var publicMethods = classSymbol.GetMembers()
                .OfType<IMethodSymbol>()
                .Where(x =>
                    x.MethodKind == MethodKind.Ordinary &&
                    x.DeclaredAccessibility == Accessibility.Public
                )
                .Select(x => {
                    var parameters = x.Parameters
                        .Select(y => new ParameterMetadata {
                            Type = y.Type,
                            TypeName = y.Type.ToDisplayString(),
                            Name = y.Name,
                            AsMemberName = ToPascalCase(y.Name)
                        })
                        .ToList();
                    var dataParams = parameters
                        .Where(y => y.TypeName != FullCancellationTokenType)
                        .ToList();
                    var ctsParam = parameters
                        .FirstOrDefault(y => y.TypeName == FullCancellationTokenType);
                    var methodName = x.Name;
                    if (methodName.EndsWith("Async")) {
                        methodName = methodName.Substring(0, methodName.Length - 5);
                    }

                    var returnType = GetReturnType(x);
                    var returnTypeName = returnType?.Name;
                    return new MethodParameterMetadata() {
                        Name = methodName,
                        OriginalName = x.Name,
                        DataParameters = dataParams,
                        CancellationTokenParameter = ctsParam,
                        ReturnType = returnType,
                        ReturnTypeName = returnTypeName
                    };
                })
                .ToList();

            var usingNamespaces = publicMethods
                .SelectMany(x => x.DataParameters.Select(y => y.Type.ContainingNamespace.ToDisplayString()))
                .Concat(publicMethods.Select(x => x.ReturnType).OfType<ITypeSymbol>().Select(x => x.ContainingNamespace.ToDisplayString()))
                .Where(x => x != "System" && x != "System.Collections.Generic" && x != "System.Threading" && x != "System.Threading.Tasks")
                .Distinct()
                .Select(x => $"using {x};");

            var commandDefinition = publicMethods.Select(x => {
                if (x.DataParameters.Count == 0 && x.ReturnType == null) {
                    return $"internal sealed class {x.Name}Command : IRequest;\r\n";
                }

                var members = string.Join("\n    ", x.DataParameters.Select(p => $"public required {p.Type} {p.AsMemberName} {{ get; init; }}"));
                if (x.DataParameters.Count > 0 && x.ReturnType == null) {
                    return $@"internal sealed class {x.Name}Command : IRequest {{
    {members}
}}
";
                }

                return $@"internal sealed class {x.Name}Command : IRequest<{x.ReturnTypeName}, {x.Name}Command.Reply> {{
    {members}

    public sealed class Reply : IReply<{x.ReturnTypeName}> {{
        public required {x.ReturnTypeName} State {{ get; init; }}
    }}
}}
";
            });

            var commandSwitch = publicMethods.Select(x => {
                var loweCaseName = x.Name.Substring(0, 1).ToLowerInvariant() + x.Name.Substring(1);
                var callParams = string.Join(", ", x.DataParameters.Select(y => $"{loweCaseName}.{y.AsMemberName}"));
                if (x.CancellationTokenParameter != null) {
                    callParams += ", letter.CancellationToken";
                }

                if (x.ReturnType == null) {
                    return $@"                case {x.Name}Command {loweCaseName}:
                    await _grain.{x.OriginalName}({callParams});
                    letter.Sender.Tell(SuccessReply.Instance);
                    break;";
                }
                return $@"                case {x.Name}Command {loweCaseName}:
                    var {loweCaseName}Result = await _grain.{x.OriginalName}({callParams});
                    letter.Sender.Tell(new {x.Name}Command.Reply() {{
                        State = {loweCaseName}Result
                    }});
                    break;";
            });

            var clientMethods = publicMethods.Select(x => {
                string methodParams = string.Join(", ", x.DataParameters.Select(y => $"{y.Type} {y.Name}"));
                if (x.CancellationTokenParameter != null) {
                    methodParams += $", {CancellationTokenType} {x.CancellationTokenParameter.Name} = default";
                }

                if (x.ReturnTypeName != null) {
                    return $@"    public async ValueTask<{x.ReturnTypeName}> {x.OriginalName}({methodParams}) {{
        return await _actor.Ask(new {x.Name}Command() {{
{string.Join(",\n", x.DataParameters.Select(y => $"            {y.AsMemberName} = {y.Name}"))}
        }}, cancellationToken: {x.CancellationTokenParameter?.Name ?? "default"});
    }}";
                }
                return $@"    public async ValueTask {x.OriginalName}({methodParams}) {{
        await _actor.Ask(new {x.Name}Command() {{
{string.Join(",\n", x.DataParameters.Select(y => $"            {y.AsMemberName} = {y.Name}"))}
        }}, cancellationToken: {x.CancellationTokenParameter?.Name ?? "default"});
    }}";
            });

            // Build up the source code
            var code = $@"// <auto-generated/>

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ChatApp.Common.Actors.Abstractions;
using {GrainNamespace};
{string.Join("\n", usingNamespaces)}

namespace {namespaceName};

{string.Join("\n", commandDefinition)}
public sealed class {className}Client {{
    private readonly IActorRef _actor;

    public {className}Client(IRequiredActor<{className}Actor> actor) {{
        _actor = actor.ActorRef;
    }}

{string.Join("\n\n", clientMethods)}
}}

public sealed class {className}Actor : IActor {{
    private readonly {originalClassName} _grain;

    public {className}Actor(IActorContext context) {{
        _grain = new {originalClassName}();
    }}

    public async ValueTask OnLetter(Envelope letter) {{
        try {{
            switch (letter.Body) {{
                case InitiateCommand:
                    await ((IGrain)_grain).OnActivate(letter.CancellationToken);
                    letter.Sender.Tell(SuccessReply.Instance);
                    break;
                case PassivateCommand:
                    await ((IGrain)_grain).OnDeactivate(letter.CancellationToken);
                    letter.Sender.Tell(SuccessReply.Instance);
                    break;
{string.Join("\n", commandSwitch)}
            }}
        }} catch (Exception ex) {{
            letter.Sender.Tell(new FailureReply(ex));
        }}
    }}
}}
";

            // Add the source code to the compilation.
            context.AddSource($"{className}Client.g.cs", SourceText.From(code, Encoding.UTF8));
        }
    }

    private static string ToPascalCase(string name) {
        return char.ToUpperInvariant(name[0]) + name.Substring(1);
    }

    private sealed record MethodParameterMetadata {
        public string Name { get; set; } = string.Empty;
        public string OriginalName { get; set; } = string.Empty;
        public IReadOnlyList<ParameterMetadata> DataParameters { get; set; } = [];
        public ParameterMetadata? CancellationTokenParameter { get; set; }
        public ITypeSymbol? ReturnType { get; set; }
        public string? ReturnTypeName { get; set; }
    }

    private sealed record ParameterMetadata {
        public ITypeSymbol Type { get; set; } = null!;
        public string TypeName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string AsMemberName { get; set; } = string.Empty;
    }
}
