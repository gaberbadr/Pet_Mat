==> make sure you download : docker (make sure is docker open while this steps or work on API)
1.  download the folder of this drive : https://drive.google.com/drive/folders/1yIxUOk5G3yVs2QEGN16hzFDZc5yK2CSM?usp=drive_link and extract it if is folder.zip
2.  open CMD of this folder path.
3.  run : docker compose -f docker-compose-production.yml up -d (wait until finish)
-the API work on : http://localhost:5000
-the API work on : http://localhost:5000/swagger/index.html


/*This makes the image up to date, meaning when you run the command prompt (cmd), it downloads the image, updates it if you have it installed, and doesn't do anything if it's already popular.*/

docker compose -f docker-compose-production.yml pull && docker compose -f docker-compose-production.yml up -d --force-recreate


//.env

# API Port (external port on your machine)
API_PORT=

# Database Port (external port on your machine)
DB_PORT=

# Database Password for sql server connection string and mcr.microsoft.com/mssql/server:2022-latest password on docker
DB_PASSWORD=

# Database Configuration (for Docker Compose)
ConnectionStrings__DefaultConnection=Server=db;Database=;User Id=sa;Password=${DB_PASSWORD};TrustServerCertificate=True;MultipleActiveResultSets=true;Connection Timeout=30;

# JWT Configuration
JWT__Key=
JWT__Issuer=http://localhost:5000/
JWT__Audience=PetMat
JWT__AccessTokenExpirationMinutes=15
JWT__RefreshTokenExpirationDays=7

# SMTP Configuration
SMTP__Host=smtp.gmail.com
SMTP__Port=587
SMTP__Email=
SMTP__Password=

# Google OAuth Configuration
Authentication__Google__ClientId=
Authentication__Google__ClientSecret=

# Application Configuration
BaseURL=
Frontend__BaseUrl=
ASPNETCORE_URLS=

# Stripe Configuration
Stripe__Secretkey=
Stripe__WebhookSecret=


---------------------------------

(appsettings.json) if needed
{
    "ConnectionStrings": {
        "DefaultConnection": "Server=.;Database=yourDB;Trusted_Connection=True;TrustServerCertificate=True"
    },
    "JWT": {
        "Key": "",
        "Issuer": "http://localhost:5000/",
        "Audience": "",
        "AccessTokenExpirationMinutes": 15,
        "RefreshTokenExpirationDays": 7
    },
    "Smtp": {
        "Host": "smtp.gmail.com",
        "Port": 587,
        "Email": "yourgmail@gmail.com",
        "Password": ""
    },
    "Authentication": {
        "Google": {
            "ClientId": "",
            "ClientSecret": ""
        }
    },
    "Stripe": {
        "Secretkey": "sk_test_5",
        "WebhookSecret": "whsec_4f"
    },
    "Logging": {
        "LogLevel": {
            "Default": "Information",
            "Microsoft.AspNetCore": "Warning"
        }
    },
    "FCM": {
        "CredentialsPath": "firebase-credentials.json",
        "ProjectId": ""
    },
    "AllowedHosts": "*",
    "BaseURL": "http://localhost:5105/",
    "Frontend": {
        "BaseUrl": "http://127.0.0.1:5500"
    }
}


---------------


