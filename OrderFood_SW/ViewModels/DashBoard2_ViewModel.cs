namespace OrderFood_SW.ViewModels
{
    public class OrderStatisticViewModel
    {
        // Tổng số đơn
        public int Approved { get; set; }
        public int Cancelled { get; set; }
        public int Pending { get; set; }

        // Thống kê theo tuần
        public List<int> WeeklyApproved { get; set; } = new List<int>();
        public List<int> WeeklyCancelled { get; set; } = new List<int>();
        public List<int> WeeklyPending { get; set; } = new List<int>();
    }
}
