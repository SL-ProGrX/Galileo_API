using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX.Activos_Fijos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;

namespace Galileo.BusinessLogic.ProGrX_Activos_Fijos
{
    public class FrmActivosPersonasBL
    {
        private readonly FrmActivosPersonasDB _db;

        public FrmActivosPersonasBL(IConfiguration config)
        {
            _db = new FrmActivosPersonasDB(config);
        }

        public ErrorDto<ActivosPersonasLista> Activos_Personas_Lista_Obtener(int CodEmpresa, string jfiltros, string codDepartamento, string codSeccion)
        {
            FiltrosLazyLoadData? filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros);
            if (filtros == null)
            {
                filtros = new FiltrosLazyLoadData();
            }
            return _db.Activos_Personas_Lista_Obtener(CodEmpresa, filtros, codDepartamento, codSeccion);
        }

        public ErrorDto<List<ActivosPersonasData>> Activos_Personas_Obtener(int CodEmpresa, string jfiltros, string codDepartamento, string codSeccion)
        {
            FiltrosLazyLoadData? filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros);
            if (filtros == null)
            {
                filtros = new FiltrosLazyLoadData();
            }
            return _db.Activos_Personas_Obtener(CodEmpresa, filtros, codDepartamento, codSeccion);
        }

        public ErrorDto Activos_Personas_Guardar(int CodEmpresa, string usuario, ActivosPersonasData persona)
        {
            return _db.Activos_Personas_Guardar(CodEmpresa, usuario, persona);
        }

        public ErrorDto Activos_Personas_Eliminar(int CodEmpresa, string identificacion, string usuario)
        {
            return _db.Activos_Personas_Eliminar(CodEmpresa, identificacion, usuario);
        }

        public ErrorDto Activos_Personas_Valida(int CodEmpresa, string identificacion)
        {
            return _db.Activos_Personas_Valida(CodEmpresa, identificacion);
        }

        public ErrorDto<CambioDeptoResponse> Activos_Personas_CambioDepto_Aplicar(int CodEmpresa, string usuario, CambioDeptoRequest request)
        {
            return _db.Activos_Personas_CambioDepto_Aplicar(CodEmpresa, usuario, request);
        }

        public ErrorDto Activos_Personas_SincronizarRH(int CodEmpresa, string usuario)
        {
            return _db.Activos_Personas_SincronizarRH(CodEmpresa, usuario);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Departamentos_Obtener(int CodEmpresa)
        {
            return _db.Activos_Departamentos_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Secciones_ObtenerPorDepto(int CodEmpresa, string cod_departamento)
        {
            return _db.Activos_Secciones_ObtenerPorDepto(CodEmpresa, cod_departamento);
        }

        public ErrorDto<object> Activos_BoletaActivosAsignados_Lote(int CodEmpresa, ActivosPersonasReporteLoteRequest request)
        {
            return _db.Activos_BoletaActivosAsignados_Lote(CodEmpresa, request);
        }

        public ErrorDto<object> Activos_ContratoResponsabilidad_Lote(int CodEmpresa, ActivosPersonasReporteLoteRequest request)
        {
            return _db.Activos_ContratoResponsabilidad_Lote(CodEmpresa, request);
        }

    }
}