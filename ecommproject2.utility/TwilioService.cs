using Microsoft.Extensions.Configuration;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using static System.Net.WebRequestMethods;
public class TwilioService
{
    private readonly string _accountSid;
    private readonly string _authToken;
    private readonly string _fromNumber;

    public TwilioService(IConfiguration config)
    {
        _accountSid = config["Twilio:AccountSid"];
        _authToken = config["Twilio:AuthToken"];
        _fromNumber = config["Twilio:FromNumber"];

        TwilioClient.Init(_accountSid, _authToken);
    }
    public void SendSms(string to, string message)
    {
        try
        {
            var result = MessageResource.Create(
                to: new PhoneNumber("+919816991300"),
                from: new PhoneNumber("+16204661904"),
                body: message
            );

            Console.WriteLine($"SMS sent: SID = {result.Sid}, Status = {result.Status}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Twilio error: {ex.Message}");
        }
    }
    public void MakeCall(string to, string twimlUrl)   
    {
        try
        {
            var call = CallResource.Create(
                to: new PhoneNumber("+919816991300"),
                from: new PhoneNumber("+16204661904"),
                url: new Uri("https://handler.twilio.com/twiml/EH41d9b202266607ec42d627b76e5c2da7") // This must be the TwiML Bin URL
            );

            Console.WriteLine($"Call initiated: SID = {call.Sid}, Status = {call.Status}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Twilio call error: {ex.Message}");
        } 
    }
    public async Task MakeRefundedCallAsync(string toPhoneNumber, int orderId)
    {
        var call = await CallResource.CreateAsync(
            to: new PhoneNumber("+919816991300"),
            from: new PhoneNumber("+16204661904"),
            twiml: new Twiml($"<Response><Say>Your order with ID {orderId} has been refunded successfully. Thank you.</Say></Response>")
        );
    }
}


