namespace cinema_app_back.DTOs
{
    public class ReserveDto
    {
        public int Id { get; set; }
        public int ShowtimeId { get; set; }
        public int UserId { get; set; }
        public int SeatId { get; set; }
    }
}
