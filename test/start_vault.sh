#!/bin/bash

# If you wish to stop Vault
if [[ $1 == "stop" ]]
then
    docker stop vault
    exit

# If you just want to run a fresh installation of Vault
elif [[ $1 == "restart" ]]
then
    docker stop vault
fi

runsOnGitHub=false

# Determines if script is running on Github or locally
if [[ $GITHUB_RUN_ID ]]
then
    runsOnGitHub=true
fi

VAULT_PORT=8200
export VAULT_ADDR='http://127.0.0.1:'$VAULT_PORT
export VAULT_TOKEN='root'

# If the script is not running on Github, start docker image
if [[ $runsOnGitHub == false ]]
then
    echo "Starting Docker with Vault"
    docker run --rm -it -d -p $VAULT_PORT:$VAULT_PORT --name vault hashicorp/vault:latest server -dev -dev-root-token-id=$VAULT_TOKEN
    sleep 10
fi

echo "Authenticate with Vault"
vault login $VAULT_TOKEN

echo "Write secrets to Vault"
vault secrets enable -path=app/myorg kv-v2
vault kv put app/myorg/mssql/backoffice host="mydbserver" port="1433" catalog="master" username="sa" password="password123" > /dev/null
vault kv put app/myorg/mssql/clients \
    _default='Data Source=mydbserver,1433;initial catalog=master;persist security info=True;user id=sa;password=password123' \
    client1='Data Source=mydbserver,1433;initial catalog=master;persist security info=True;user id=sa;password=password123' > /dev/null

echo "Create policy to allow read of secrets"
vault write /sys/policy/dev-deployments policy='path "app/myorg/*" { capabilities = ["read"] }'

echo "Enable AppRole auth method"
vault auth enable approle
vault write auth/approle/role/myorg policies=default,dev-deployments period=1h

echo "Create and get Role ID and Secret ID"
ROLE_ID=$(vault read -field=role_id auth/approle/role/myorg/role-id | cat -v | sed 's|\^\[\[0m||g' | sed 's|\^M||g')
SECRET_ID=$(vault write -f -field=secret_id auth/approle/role/myorg/secret-id | cat -v | sed 's|\^\[\[0m||g' | sed 's|\^M||g')

mkdir -p ~/.myorg/secrets
echo "${ROLE_ID}" > ~/.myorg/secrets/approle.role_id
echo "${SECRET_ID}" > ~/.myorg/secrets/approle.secret_id
