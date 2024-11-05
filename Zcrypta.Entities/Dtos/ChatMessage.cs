namespace Zcrypta.Entities.Dtos
{
    public class ChatMessage
    {
        public long Id { get; set; }
        public long SenderUserId { get; set; }
        public long ReceiverUserId { get; set; }
        public string Message { get; set; }
        public DateTime SentDateTime { get; set; }
        public DateTime ReceivedDateTime { get; set; }
        public DateTime CreateDate { get; set; }

    }
}
