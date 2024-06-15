namespace Plan_A_Plant.Utility
{
    public interface IEmailService
    {
        void SendEmail(string email, string v, Message message);
        void SendEmail(Message mes);
    }
}
