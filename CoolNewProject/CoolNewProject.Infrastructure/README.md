# Pulumi
## Pulumi Setup (State)
```
set AZURE_STORAGE_ACCOUNT=swisslogdevgeneral
set AZURE_STORAGE_KEY=$KeyFromKeeperOrAzure
set PULUMI_CONFIG_PASSPHRASE=$KeyFromKeeper
az login
# MPN Simon 150$ sub
az account set --subscription=5c833e65-662f-4931-970f-492aa4e6ebf0
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
pulumi config set --path azure-native:sql.password --secret $KeyFromKeeper
```