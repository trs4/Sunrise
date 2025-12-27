publish

net8.0-android  self-contained  android-arm64

archive

cd C:\Projects\Sunrise\Sunrise.Android

keytool -genkeypair -v -keystore Sunrise.keystore -alias Sunrise -keyalg RSA -keysize 2048 -validity 10000

am
am
am
ru
ru
ru
y

dotnet publish -f net8.0-android -c Release -p:AndroidKeyStore=true -p:AndroidSigningKeyStore=Sunrise.keystore -p:AndroidSigningKeyAlias=Sunrise -p:AndroidSigningKeyPass={password} -p:AndroidSigningStorePass={password}

use C:\Projects\Sunrise\bin\Release\net8.0-android
com.am.sunrise-Signed.apk

Брандмауэр правила для портов:
21450 UDP
21451 TCP