using Paycor.SystemCheck;

namespace Paycor.Import.Web.Models
{
    public class PulseCheckModel
    {
        public string Pulse { get; set; }

        public PulseCheckModel()
        {
            Pulse = PulseCheckHelper.PulseCheck();
        }
    }
}