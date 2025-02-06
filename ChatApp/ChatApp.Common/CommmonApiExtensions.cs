using System.Text;

namespace ChatApp.Common;

public static class StringExtensions {
    public static string ToKebabCase(this string value) {
        var stringBuilder = new StringBuilder();
        for (var i = 0; i < value.Length; i++) {
            var c = value[i];
            if (char.IsUpper(c)) {
                if (i > 0) {
                    stringBuilder.Append('-');
                }
                stringBuilder.Append(char.ToLower(c));
            } else {
                stringBuilder.Append(c);
            }
        }
        return stringBuilder.ToString();
    }
}
