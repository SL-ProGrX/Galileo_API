using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX.Cobros;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cobros;

namespace Galileo.BusinessLogic.ProGrX.Cobros
{
    public class FrmCOControlGestionesBL
    {
        private readonly IConfiguration? _config;
        private readonly FrmCOControlGestionesDB _db;

        public FrmCOControlGestionesBL(IConfiguration config)
        {
            _config = config;
            _db = new FrmCOControlGestionesDB(_config);
        }

        public ErrorDto<CoControlGestionesLista> Co_GestionesLista_Obtener(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData? filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros);
            if (filtros == null)
            {
                filtros = new FiltrosLazyLoadData();
            }
            return _db.Co_GestionesLista_Obtener(CodEmpresa, filtros);
        }
        public ErrorDto<CoControlGestionesLista> Co_Gestiones_Export(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData? filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros);

            if (filtros == null)
            {
                filtros = new FiltrosLazyLoadData();
            }

            filtros.pagina = 0;
            filtros.paginacion = 0;

            return _db.Co_GestionesLista_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto Co_Gestiones_Guardar(int CodEmpresa, string usuario, CoControlGestionesData gestion)
        {
            return _db.Co_Gestiones_Guardar(CodEmpresa, usuario, gestion);
        }

        public ErrorDto Co_Gestiones_Eliminar(int CodEmpresa, string usuario, string cod_gestion)
        {
            return _db.Co_Gestiones_Eliminar(CodEmpresa, usuario, cod_gestion);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Co_NivelGestion_Obtener(int CodEmpresa)
        {
            return _db.Co_NivelGestion_Obtener(CodEmpresa);
        }

        public ErrorDto<List<CoControlGestionesSeguridadGestionData>> Co_Seguridad_Gestiones_Obtener(int CodEmpresa)
        {
            return _db.Co_Seguridad_Gestiones_Obtener(CodEmpresa);
        }

        public ErrorDto<List<CoControlGestionesSeguridadUsuarioData>> Co_Seguridad_Usuarios_Obtener(int CodEmpresa, string cod_gestion)
        {
            return _db.Co_Seguridad_Usuarios_Obtener(CodEmpresa, cod_gestion);
        }

        public ErrorDto Co_Seguridad_Asignacion_Guardar(int CodEmpresa, string usuario, CoControlGestionesSeguridadAsignacionDto dto)
        {
            return _db.Co_Seguridad_Asignacion_Guardar(CodEmpresa, usuario, dto);
        }
    }
}