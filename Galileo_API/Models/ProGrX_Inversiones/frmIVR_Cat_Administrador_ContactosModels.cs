namespace Galileo.Models.ProGrX_Inversiones
{
    public sealed class
        IvrCatAdministradorContactoNavegacionData
    {
        public int cod_contacto { get; set; } = 0;
    }

    public sealed class
        IvrCatAdministradorContactoGuardarRequest
    {
        public int cod_contacto { get; set; } = 0;

        public string cod_administrador { get; set; } =
            string.Empty;

        public string nombre { get; set; } =
            string.Empty;

        public string celular { get; set; } =
            string.Empty;

        public string telefono { get; set; } =
            string.Empty;

        public string email_01 { get; set; } =
            string.Empty;

        public string email_02 { get; set; } =
            string.Empty;

        public string usuario { get; set; } =
            string.Empty;
    }

    public sealed class
        IvrCatAdministradorContactoEliminarRequest
    {
        public string cod_administrador { get; set; } =
            string.Empty;

        public int cod_contacto { get; set; } = 0;

        public string usuario { get; set; } =
            string.Empty;
    }
}
