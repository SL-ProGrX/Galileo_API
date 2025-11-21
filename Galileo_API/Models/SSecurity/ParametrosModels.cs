namespace Galileo.Models.Security
{
    public class ParametrosDto
    {
        public int ID_PARAMETRO { get; set; }
        public int KEY_LENMIN { get; set; }
        public int KEY_LENMAX { get; set; }
        public int KEY_RENEW_DAY { get; set; }
        public int KEY_REMAIN_DAYS { get; set; }
        public int KEY_HISTORY { get; set; }
        public int TIME_LOCK { get; set; }
        public int KEY_INTENTOS { get; set; }
        public int KEY_CAPCHAR { get; set; }
        public int KEY_SIMCHAR { get; set; }
        public int KEY_NUMCHAR { get; set; }
    }
}
