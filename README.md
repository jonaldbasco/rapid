Environment Variable for Negotiate Agent
GMAIL_SMTP_FROM - symplraico@gmail.com
GMAIL_SMTP_PASS - akvryachiebiyuai
GMAIL_SMTP_USER - symplraico@gmail.com
OPENAI_API_KEY - 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

Setup Ngrok for Public API

"C:\Program Files\WindowsApps\ngrok.ngrok_*\ngrok.exe" config add-authtoken 39OopDqfe8DQQW2iCfifUVVzboF_54w3sAWLHGWuE73rPpDtc

setx PUBLIC_BASE_URL "https://claudio-nonvisual-noncannibalistically.ngrok-free.dev "

ngrok http https://localhost:7014
