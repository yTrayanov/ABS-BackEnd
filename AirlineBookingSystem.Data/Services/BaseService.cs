namespace AirlineBookingSystem.Data.Services
{
    public abstract class BaseService
    {
        public BaseService(ABSContext context)
        {
            this.Context = context;
        }
        protected ABSContext Context { get; set; }


    }
}
