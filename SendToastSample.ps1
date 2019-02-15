$clientId = "ms-app%3a%2f%2fS-1-15-2-2390588901-1971267905-4164622345-4160125855-2178670139-1585114836-3156657637"
$secret = "N%2BfhcyiubiiUMlqosglFkaSyHzlkqyKP"
$body = "grant_type=client_credentials&client_id=$clientId&client_secret=$secret&scope=notify.windows.com"
$resp = Invoke-WebRequest -Method Post -Uri https://login.live.com/accesstoken.srf -ContentType "application/x-www-form-urlencoded" -Body $body
"Get Token Status: $($resp.StatusCode)"
$token = $resp.Content | ConvertFrom-Json

$notifyUri = "https://bn2.notify.windows.com/?token=AwYAAAByMhptM8G78Psgk5UQTT%2bTpy4LpMzpMDgGNMXBcEAKFmGP0Q%2bAMKf5uSSopsLGatGlb2JUrPVBCZOpxMUAUWukjDX749spfWBCiTN6Z%2fgXBk%2fBlxEKXBkG2O%2b7MLPvPe%2bbrKxB4K3Smcwln0fcR18j"
$headers = @{
    "Authorization" = "Bearer $($token.access_token)";
    "X-WNS-Type" = "wns/toast"
}
$notifyBody = '<?xml version="1.0" encoding="utf-8"?>
<toast launch="action=viewConversation&amp;conversationId=384928">
    <visual>
        <binding template="ToastGeneric">
            <text>Andrew sent you a picture</text>
            <text>Check this out, Happy Canyon in Utah!</text>
            <image src="https://picsum.photos/364/180?image=1043" placement="hero" />
            <image src="ms-appx:///leaf.jpg" placement="appLogoOverride" hint-crop="circle" />
        </binding>
    </visual>
</toast>'
$sendResp = Invoke-WebRequest -Method Post -Uri $notifyUri -ContentType "text/xml" -Headers $headers -Body $notifyBody
"Send notification status: $($sendResp.StatusCode)"
$sendResp.Content