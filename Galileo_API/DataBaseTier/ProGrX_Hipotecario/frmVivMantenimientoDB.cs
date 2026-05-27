using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Hipotecario;

namespace Galileo_API.DataBaseTier.ProGrX_Hipotecario
{
    public partial class FrmVivMantenimientoDb
    {
        private readonly PortalDB _portalDb;

        public FrmVivMantenimientoDb(IConfiguration config)
            : this(new PortalDB(config))
        {
        }

        public FrmVivMantenimientoDb(PortalDB portalDb)
        {
            _portalDb = portalDb;
        }

        /// <summary>
        /// Obtiene los nodos principales del mantenimiento de garantias hipotecarias.
        /// </summary>
        /// <returns></returns>
        public ErrorDto<List<VivMantenimientoNodoData>> VivMantenimiento_ArbolInicial_Obtener()
        {
            var nodos = new List<VivMantenimientoNodoData>
            {
                CrearNodo("NodoParametrosGenerales", "Parametros", "NodoParametrosGenerales", "pi pi-cog", "frmVivParametros", "/viv-parametros"),
                CrearNodo("NodoZonas", "Zonas", "NodoZonas", "pi pi-map", "frmVivZonas", "/viv-zonas", leaf: false),
                CrearNodo("NodoProfesionales", "Profesionales", "NodoProfesionales", "pi pi-users", "frmVivProfesionales", "/viv-informacion-profesionales", leaf: false),
                CrearNodo("NodoTiposDesembolsos", "Conceptos Desembolsos", "NodoTiposDesembolsos", "pi pi-list", "frmVivTiposDesembolsos", "/viv-tipos-desembolsos"),
                CrearNodo("NodoTramiteGarantia", "Garantias en Tramite", "NodoTramiteGarantia", "pi pi-briefcase", "frmVivControlAsignacionGarantia", "/viv-control-asignacion-garantia"),
                CrearNodo("NodoOperacionesTramite", "Operaciones en tramite", "NodoOperacionesTramite", "pi pi-file-edit"),
                CrearNodo("NodoTiemposSeguimiento", "Tiempos Seguimiento", "NodoTiemposSeguimiento", "pi pi-clock", "frmVivTiemposSeguimiento", "/viv-tiempos-seguimiento"),
                CrearNodo("NodoControlDesembolso", "Control Desembolso", "NodoControlDesembolso", "pi pi-wallet", "frmVivDesembolsos", "/viv-desembolsos"),
                CrearNodo("NodoOperacionesCanceladas", "Operaciones Canceladas", "NodoOperacionesCanceladas", "pi pi-ban")
            };

            return DbHelper.CreateOkResponse(nodos);
        }

        private static VivMantenimientoNodoData CrearNodo(
            string key,
            string label,
            string tag,
            string icon,
            string formulario = "",
            string ruta = "",
            bool leaf = true)
        {
            return new VivMantenimientoNodoData
            {
                key = key,
                label = label,
                tag = tag,
                icon = icon,
                formulario = formulario,
                ruta = ruta,
                leaf = leaf
            };
        }
    }
}
