using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.DataBaseTier.ProGrX.Bancos;

namespace Galileo_API.BusinessLogic
{
    public class FrmTesBancosBL
    {

        private readonly FrmTesBancosDB BancosDb;

        public FrmTesBancosBL(IConfiguration config)
        {
            BancosDb = new FrmTesBancosDB(config);
        }

        public ErrorDto<TesBancoDto> TES_Banco_Obtener(int CodEmpresa, int Contabilidad, int Banco)
        {
            return BancosDb.TES_Banco_Obtener(CodEmpresa, Contabilidad, Banco);
        }

        public ErrorDto<TesBancoDto> TES_Bancos_Scroll_Obtener(int CodEmpresa, int Contabilidad, int scrollCode, int Banco)
        {
            return BancosDb.TES_Bancos_Scroll_Obtener(CodEmpresa, Contabilidad, scrollCode, Banco);
        }

        public ErrorDto<TablasListaGenericaModel> TES_Bancos_Lista_Obtener(int CodEmpresa, string filtro)
        {
            return BancosDb.TES_Bancos_Lista_Obtener(CodEmpresa, filtro);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TES_Bancos_Grupos_Obtener(int CodEmpresa)
        {
            return BancosDb.TES_Bancos_Grupos_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaDivisas>> TES_Bancos_Divisas_Obtener(int CodEmpresa)
        {
            return BancosDb.TES_Bancos_Divisas_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TES_Bancos_Formatos_Obtener(int CodEmpresa)
        {
            return BancosDb.TES_Bancos_Formatos_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TES_Bancos_Unidades_Obtener(int CodEmpresa)
        {
            return BancosDb.TES_Bancos_Unidades_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TES_Bancos_CentrosCostos_Obtener(int CodEmpresa)
        {
            return BancosDb.TES_Bancos_CentrosCostos_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TES_Bancos_Conceptos_Obtener(int CodEmpresa)
        {
            return BancosDb.TES_Bancos_Conceptos_Obtener(CodEmpresa);
        }

        public ErrorDto<List<TesBancosCierres>> TES_Bancos_Cierres_Obtener(int CodEmpresa, int Banco)
        {
            return BancosDb.TES_Bancos_Cierres_Obtener(CodEmpresa, Banco);
        }

        public ErrorDto<int> TES_Bancos_Guardar(int CodEmpresa, bool vEdita, string Usuario, TesBancoDto Parametros)
        {
            return BancosDb.TES_Bancos_Guardar(CodEmpresa, vEdita, Usuario, Parametros);
        }

        public ErrorDto TES_Bancos_Borrar(int CodEmpresa, int Banco, string Usuario)
        {
            return BancosDb.TES_Bancos_Borrar(CodEmpresa, Banco, Usuario);
        }

        public ErrorDto TES_Bancos_RangoFirmas_Actualizar(int CodEmpresa, int Banco, int FirmaDesde, int FirmaHasta, string Usuario)
        {
            return BancosDb.TES_Bancos_RangoFirmas_Actualizar(CodEmpresa, Banco, FirmaDesde, FirmaHasta, Usuario);
        }

        public ErrorDto TES_Bancos_SaldoFecha_Actualizar(int CodEmpresa, string Parametros)
        {
            return BancosDb.TES_Bancos_SaldoFecha_Actualizar(CodEmpresa, Parametros);
        }

        public ErrorDto TES_Bancos_Conciliacion_Actualizar(int CodEmpresa, string Parametros)
        {
            return BancosDb.TES_Bancos_Conciliacion_Actualizar(CodEmpresa, Parametros);
        }

        public ErrorDto<List<TesBancosGruposAsgDto>> TES_BancosGrupos_Lista(int CodEmpresa, int id_banco)
        {
            return BancosDb.TES_BancosGrupos_Lista(CodEmpresa, id_banco);
        }

        public ErrorDto TES_BancosGrupos_Asignar(int CodEmpresa, int id_banco, bool asigna, TesBancosGruposAsgDto grupo)
        {
            return BancosDb.TES_BancosGrupos_Asignar(CodEmpresa, id_banco, asigna, grupo);
        }

        public async Task<ErrorDto> TES_BancosArchivos_Subir(
          int codEmpresa,
           int codBanco,
            string documento,
          IFormFile file)
        {
            return await BancosDb.TES_BancosArchivos_Subir(codEmpresa, codBanco, documento, file);
        }

        public ErrorDto<ArchivoDto> ResolverDocumento(int CodEmpresa, int CodBanco, string documento)
        {
            return BancosDb.ResolverDocumento(CodEmpresa, CodBanco, documento);
        }
    }
}