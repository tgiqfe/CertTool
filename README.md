# CertTool

自己証明局作成～サーバ証明書までを簡単に実装。

## 使用しているOpenSSLのバージョン確認

```powershell
Get-OpensslVersion
```

## 自己証明局 (ルートCA) 作成

↓の場合の例
- サブジェクト&nbsp;：&nbsp;"/C=JP/OU=example/CN=ca.example.local.net"
- ルートCA用証明書ファイル&nbsp;：&nbsp;ca.crt
- ルートCA用鍵ファイル&nbsp;：&nbsp;ca.key
- 有効期限&nbsp;：&nbsp;365日
- RSAビット数&nbsp;：&nbsp;4096ビット

```powershell
New-RootCertificateAuthority -CACrtFile ".\ca.crt" -CAKeyFile ".\ca.key" -ExpireDays 365 -RsaBits 4096
```

## サーバ証明書用CSR作成

↓の場合の例

- サブジェクト&nbsp;：&nbsp;"/C=JP/OU=example/CN=sv01.example.local.net"
- サブジェクトの代替名&nbsp;：
    - aaaa.example.local.net
    - bbbb.example.local.net
    - cccc.example.local.net
- CSRファイル&nbsp;：&nbsp;server.csr
- 鍵ファイル&nbsp;：&nbsp;server.key
- RSAビット数&nbsp;：&nbsp;4096ビット

```powershell
New-CertificateSingningRequest -Subject "/C=JP/OU=example/CN=sv01.example.local.net" -AlternateNames @("aaaa.example.local.net", "bbbb.example.local.net", "cccc.example.local.net") -CsrFile ".\server.csr" -KeyFile ".\server.key" -RsaBits 4096
```

## CSRに署名してサーバ証明書を作成

↓の場合の例

- ルートCA用証明書ファイル&nbsp;：&nbsp;ca.crt
- ルートCA用鍵ファイル&nbsp;：&nbsp;ca.key
- CSRファイル&nbsp;：&nbsp;server.csr
- サーバ証明書ファイル&nbsp;：&nbsp;server.crt
- 有効期限&nbsp;：&nbsp;365日

```powershell
New-ServerCertificate -CACrtFile ".\ca.crt" -CAKeyFile ".\ca.key" -CsrFile ".\server.csr" -CrtFile ".\server.crt" -ExpireDays 365
```

## 作成したサーバ証明書を破棄

※証明書失効リストとかは未対応。

↓の場合の例

- サーバ証明書ファイル&nbsp;：&nbsp;server.crt
- ルートCA用証明書ファイル&nbsp;：&nbsp;ca.crt
- ルートCA用鍵ファイル&nbsp;：&nbsp;ca.key

```powershell
Revoke-ServerCertificate -CrtFile ".\server.crt" -CACrtFile ".\ca.crt" -CAKeyFile ".\ca.key"
```

## CSR/証明書/鍵ファイルの内容をテキストに変換

```powershell
# 基本構文 ※拡張子からCSR/証明書/鍵ファイルを自動判定
Convert-CertificateToText -Path <対象ファイル>

# CSRでと指定してテキスト変換
Convert-CertificateToText -Path <対象ファイル> -Csr

# 証明書ファイルと指定してテキスト変換
Convert-CertificateToText -Path <対象ファイル> -Crt

# 鍵ファイルと指定してテキスト変換
Convert-CertificateToText -Path <対象ファイル> -Key
```

