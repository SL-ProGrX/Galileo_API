namespace Galileo_API.Models.ProGrX_Hipotecario
{
    public abstract class VivControlGarantiaBaseData
    {
        public int idGarantia { get; set; } = 0;
        public int numeroOperacion { get; set; } = 0;
        public string cod_preanalisis { get; set; } = string.Empty;
        public decimal montoapr { get; set; } = 0;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string numeroFinca { get; set; } = string.Empty;
        public string numPlanoCatastro { get; set; } = string.Empty;
        public string tipoDerecho { get; set; } = string.Empty;
        public string descGradoHipoteca { get; set; } = string.Empty;
        public decimal areaFinca { get; set; } = 0;
        public string descZona { get; set; } = string.Empty;
        public int idZona { get; set; } = 0;
    }

    public abstract class VivControlProfesionalActionRequestBase
    {
        public int idGarantia { get; set; } = 0;
        public int idContacto { get; set; } = 0;
        public string tipoProfesional { get; set; } = string.Empty;
    }

    public class VivControlAsignacionGarantiaPendienteData : VivControlGarantiaBaseData
    {
        public string registroUsuario { get; set; } = string.Empty;
        public DateTime? registroFecha { get; set; }
    }

    public class VivControlAsignacionProfesionalData
    {
        public int idZona { get; set; } = 0;
        public int idContacto { get; set; } = 0;
        public int idGarantia { get; set; } = 0;
        public string identificacion { get; set; } = string.Empty;
        public string idEmpresa { get; set; } = string.Empty;
        public string tipoProfesional { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string profesional { get; set; } = string.Empty;
        public string nombreEmpresa { get; set; } = string.Empty;
        public int cantOp { get; set; } = 0;
        public decimal montoapr { get; set; } = 0;
        public int itemAsignado { get; set; } = 0;
        public string condicion { get; set; } = "N";
    }

    public class VivControlAsignacionGarantiaAsignarRequest : VivControlProfesionalActionRequestBase
    {
        public DateTime? fecha_asignacion { get; set; }
    }

    public class VivControlEntregaGarantiaData : VivControlGarantiaBaseData
    {
        public string asignacionUsuario { get; set; } = string.Empty;
        public DateTime? asignacionFecha { get; set; }
        public string condicion { get; set; } = "N";
        public int diasTransProfesional { get; set; } = 0;
        public DateTime? entregaFecha { get; set; }
    }

    public class VivControlEntregaGarantiaRequest : VivControlProfesionalActionRequestBase
    {
        public string aplicar { get; set; } = string.Empty;
    }

    public class VivControlAsignacionGarantiaNotaData
    {
        public string ultima_nota { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public string fecha_registro { get; set; } = string.Empty;
        public string numero_operacion { get; set; } = string.Empty;
        public string numero_finca { get; set; } = string.Empty;
    }

    public class VivControlRecibeGarantiaData : VivControlGarantiaBaseData
    {
        public string entregaUsuario { get; set; } = string.Empty;
        public DateTime? entregaFecha { get; set; }
        public string condicion { get; set; } = "N";
        public int diasTransProfesional { get; set; } = 0;
        public DateTime? recepcionFecha { get; set; }
        public DateTime? firmasFecha { get; set; }
    }

    public class VivControlRecibeGarantiaRequest : VivControlProfesionalActionRequestBase
    {
        public string aplicar { get; set; } = string.Empty;
    }

    public class VivControlRegistroGarantiaData : VivControlGarantiaBaseData
    {
        public string asignacionUsuario { get; set; } = string.Empty;
        public DateTime? asignacionFecha { get; set; }
        public int diasTransProfesional { get; set; } = 0;
        public int diasTransAbogado { get; set; } = 0;
        public DateTime? firmasFecha { get; set; }
    }

    public class VivControlRegistroGarantiaRequest : VivControlProfesionalActionRequestBase
    {
        public string aplicar { get; set; } = string.Empty;
    }

    public class VivControlTiemposSeguimientoData
    {
        public string profesional { get; set; } = string.Empty;
        public int gTMaxEntregaAbogado { get; set; }
        public int gTAlertaEntregaAbogado { get; set; }
        public int gTMaxFirmasAbogado { get; set; }
        public int gTAlertaFirmasAbogado { get; set; }
        public int gTMaxInscripcionAbogado { get; set; }
        public int gTAlertaInscripcionAbogado { get; set; }
        public int gTMaxEntregaIngeniero { get; set; }
        public int gTAlertaEntregaIngeniero { get; set; }
        public int gTMaxRecepcionIngeniero { get; set; }
        public int gTAlertaRecepcionIngeniero { get; set; }
        public int gTMaxRegistroIngeniero { get; set; }
        public int gTAlertaRegistroIngeniero { get; set; }
    }

    public class VivControlTiempoSeguimientoRowData
    {
        public string profesional { get; set; } = string.Empty;
        public string proceso { get; set; } = string.Empty;
        public int tiempoMaximo { get; set; } = 0;
        public int tiempoAlerta { get; set; } = 0;
    }

    public class VivControlHonorariosRegistraData
    {
        public bool registraHonorarios { get; set; } = false;
    }
}