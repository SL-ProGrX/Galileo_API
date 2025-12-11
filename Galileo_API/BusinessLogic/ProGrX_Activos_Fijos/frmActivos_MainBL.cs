using Galileo.DataBaseTier.ProGrX_Activos_Fijos;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;
using Galileo.Models;

namespace Galileo.BusinessLogic.ProGrX_Activos_Fijos
{
    public class FrmActivosMainBl
    {
        private readonly FrmActivosMainDb _db;

        public FrmActivosMainBl(IConfiguration config)
        {
            _db = new FrmActivosMainDb(config);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Main_Departamentos_Obtener(int CodEmpresa)
        {
            return _db.Activos_Main_Departamentos_Obtener(CodEmpresa);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Main_Secciones_Obtener(int CodEmpresa, string departamento)
        {
            return _db.Activos_Main_Secciones_Obtener(CodEmpresa, departamento);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Main_Responsable_Obtener(int CodEmpresa, string departamento, string seccion)
        {
            return _db.Activos_Main_Responsable_Obtener(CodEmpresa, departamento, seccion);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Main_Localizacion_Obtener(int CodEmpresa)
        {
            return _db.Activos_Main_Localizacion_Obtener(CodEmpresa);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Main_TipoActivo_Obtener(int CodEmpresa)
        {
            return _db.Activos_Main_TipoActivo_Obtener(CodEmpresa);
        }
        public ErrorDto<int> Activos_Main_PermiteRegistros_Validar(int CodEmpresa)
        {
            return _db.Activos_Main_PermiteRegistros_Validar(CodEmpresa);
        }
        public ErrorDto<int> Activos_Main_ForzarTipoActivo_Validar(int CodEmpresa)
        {
            return _db.Activos_Main_ForzarTipoActivo_Validar(CodEmpresa);
        }
        public ErrorDto<string> Activos_Main_NumeroPlaca_Consultar(int CodEmpresa, int orden, string placa)
        {
            return _db.Activos_Main_NumeroPlaca_Consultar(CodEmpresa, orden, placa);
        }
        public ErrorDto<List<MainHistoricoData>> Activos_Main_Historico_Consultar(int CodEmpresa, string codigo, string estadoHistorico)
        {
            return _db.Activos_Main_Historico_Consultar(CodEmpresa, codigo, estadoHistorico);
        }
        public ErrorDto<List<MainDetalleResponsablesData>> Activos_Main_DetalleResponsables_Consultar(int CodEmpresa, string placa)
        {
            return _db.Activos_Main_DetalleResponsables_Consultar(CodEmpresa, placa);
        }
        public ErrorDto<List<MainModificacionesData>> Activos_Main_Modificaciones_Consultar(int CodEmpresa, string placa)
        {
            return _db.Activos_Main_Modificaciones_Consultar(CodEmpresa, placa);
        }
        public ErrorDto<List<MainComposicionData>> Activos_Main_Composicion_Consultar(int CodEmpresa, string placa)
        {
            return _db.Activos_Main_Composicion_Consultar(CodEmpresa, placa);
        }
        public ErrorDto<List<MainPolizasData>> Activos_Main_Polizas_Consultar(int CodEmpresa, string placa)
        {
            return _db.Activos_Main_Polizas_Consultar(CodEmpresa, placa);
        }
        public ErrorDto<MainGeneralData> Activos_Main_DatosGenerales_Consultar(int CodEmpresa, string placa)
        {
            return _db.Activos_Main_DatosGenerales_Consultar(CodEmpresa, placa);
        }
        public ErrorDto<string> Activos_Main_Validaciones_Consultar(int CodEmpresa, string placa, string placaAlternativa)
        {
            return _db.Activos_Main_Validaciones_Consultar(CodEmpresa, placa, placaAlternativa);
        }
        public ErrorDto Activos_Main_Modificar(int CodEmpresa, MainGeneralData data, int aplicacionTotal, string usuario)
        {
            return _db.Activos_Main_Modificar(CodEmpresa, data, aplicacionTotal, usuario);
        }
        public ErrorDto Activos_Main_Guardar(int CodEmpresa, MainGeneralData data, string usuario)
        {
            return _db.Activos_Main_Guardar(CodEmpresa, data, usuario);
        }
        public ErrorDto Activos_Main_Eliminar(int CodEmpresa, string codigo, string usuario)
        {
            return _db.Activos_Main_Eliminar(CodEmpresa, codigo, usuario);
        }
        public ErrorDto<List<ActivosData>> Activos_Main_Obtener(int CodEmpresa)
        {
            return _db.Activos_Main_Obtener(CodEmpresa);
        }
        public ErrorDto<MainActivosTiposData> Activos_Main_TipoActivo_Consultar(int CodEmpresa, string tipo_activo)
        {
            return _db.Activos_Main_TipoActivo_Consultar(CodEmpresa, tipo_activo);
        }
        public ErrorDto<DateTime> Activos_Main_FechaUltimoCierre(int CodEmpresa)
        {
            return _db.Activos_Main_FechaUltimoCierre(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Main_Proveedores_Obtener(int CodEmpresa)
        {
            return _db.Activos_Main_Proveedores_Obtener(CodEmpresa);
        }
        public ErrorDto<string> Activos_Main_PlacaId_Consultar(int CodEmpresa)
        {
            return _db.Activos_Main_PlacaId_Consultar(CodEmpresa);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Main_DocCompas_Obtener(int CodEmpresa, string proveedor, DateTime adquisicion)
        {
            return _db.Activos_Main_DocCompas_Obtener(CodEmpresa, proveedor, adquisicion);
        }
    }
}
