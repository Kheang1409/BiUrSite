namespace Backend.Application.Messaging.Emails;

public class VerificationEmail : EmailBase
{
    public string Url { get; private set; } = string.Empty;

    private VerificationEmail(string recipient,
        string subject,
        string firstName,
        string url)
        : base(recipient, subject, firstName)
    {
        Url = url;
    }

    public static VerificationEmail Create(
        string recipient,
        string subject,
        string firstName,
        string url)
    {
        return new VerificationEmail(recipient, subject, firstName, url);
    }

    public override string Message()
    {
        return $@"
        <html>
            <head>
                <style>
                    body {{
                        font-family: Arial, sans-serif;
                        background-color: #f4f4f4;
                        padding: 20px;
                        color: #333;
                    }}
                    .container {{
                        max-width: 600px;
                        margin: auto;
                        background: #ffffff;
                        padding: 30px;
                        border-radius: 8px;
                        box-shadow: 0 0 10px rgba(0,0,0,0.1);
                        text-align: center;
                    }}
                    .logo {{
                        margin-bottom: 20px;
                    }}
                    .button {{
                        display: inline-block;
                        padding: 15px 25px;
                        font-size: 16px;
                        color: #ffffff !important;
                        background-color: #004aad;
                        text-decoration: none;
                        border-radius: 6px;
                        font-weight: bold;
                        margin: 20px 0;
                    }}
                    .footer {{
                        font-size: 12px;
                        color: #999;
                        margin-top: 40px;
                    }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='logo'>
                        <img src='https://raw.githubusercontent.com/Kheang1409/BiUrSite/refs/heads/main/frontend/src/assets/img/logo.png' alt='BiUrSite Logo' width='120'/>
                    </div>
                    <p>Dear {FirstName},</p>
                    <p>Thank you for registering! Please verify your email by clicking the button below:</p>
                    <a href='{Url}' class='button'>Verify Email</a>
                    <p>If you did not create an account, please ignore this email.</p>
                    <p>Thank you,<br/>The BiUrSite Team</p>
                    <div class='footer'>
                        &copy; {(DateTime.UtcNow.Year)} BiUrSite. All rights reserved.
                    </div>
                </div>
            </body>
        </html>";
    }
}
