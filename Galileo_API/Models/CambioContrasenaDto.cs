namespace Galileo.Models
{
    public class ParametrosObtenerDto
    {
        public int id_parametro { get; set; }
        public int key_lenmin { get; set; }
        public int key_lenmax { get; set; }
        public int key_renew_day { get; set; }
        public int key_remain_days { get; set; }
        public int key_history { get; set; }
        public int time_lock { get; set; }
        public int key_intentos { get; set; }
        public bool key_capchar { get; set; }
        public bool key_simchar { get; set; }
        public bool key_numchar { get; set; }
        public bool tfa_ind { get; set; }
        public string tfa_metodo { get; set; } = string.Empty;
    }

    public class ClaveCambiarDto
    {
        public long Cliente { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string PassViejo { get; set; } = string.Empty;
        public string PassNuevo { get; set; } = string.Empty;
        public int Renueva { get; set; }
    }

}
