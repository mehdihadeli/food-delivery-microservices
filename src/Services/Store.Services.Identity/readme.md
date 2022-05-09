# Generating Keys
- [JWT Authentication with Asymmetric Encryption using certificates in ASP.NET Core](https://dev.to/eduardstefanescu/jwt-authentication-with-asymmetric-encryption-using-certificates-in-asp-net-core-2o7e)
- [Generate self-signed certificates with the .NET CLI](https://docs.microsoft.com/en-us/dotnet/core/additional-tools/self-signed-certificates-guide)
- [Convert .pem to .crt and .key](https://stackoverflow.com/questions/13732826/convert-pem-to-crt-and-key)
- [How to Generate a Self-Signed Certificate and Private Key using OpenSSL](https://helpcenter.gsx.com/hc/en-us/articles/115015960428-How-to-Generate-a-Self-Signed-Certificate-and-Private-Key-using-OpenSSL)

``` cmd
openssl genpkey -algorithm RSA -out private_key.pem -pkeyopt rsa_keygen_bits:2048
```