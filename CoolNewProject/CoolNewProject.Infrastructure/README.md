# Pulumi
## Pulumi Setup (State)
```
set AZURE_STORAGE_ACCOUNT=$StorageAccountName
set AZURE_STORAGE_KEY=$StorageAccountPassword
set PULUMI_CONFIG_PASSPHRASE=$PulumiPassphrase
az login
az account set --subscription=$AzureSubId
pulumi login azblob://pulumi-state
```

## Infrastructure Creation
```
pulumi up
```

## Helpful Commands
Show resources with urn
```
pulumi stack --show-urns
```

Delete single resource
```
pulumi destroy -t $URN
```

Set nested password
```
pulumi config set --path azure-native:sql.password --secret $Secret
```