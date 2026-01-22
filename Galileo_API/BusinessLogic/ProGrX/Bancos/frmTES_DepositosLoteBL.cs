using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.TES;
using Galileo_API.DataBaseTier.ProGrX.Bancos;

namespace Galileo_API.BusinessLogic.ProGrX.Bancos
{
    public class FrmTesDepositosLoteBL
    {
        private readonly FrmTesDepositosLoteDB depositosLoteDB;

        public FrmTesDepositosLoteBL(IConfiguration config)
        {
            depositosLoteDB = new FrmTesDepositosLoteDB(config);
        }

        public ErrorDto<List<TesCuentaBancariaDto>> TES_DepositosLote_Ctas_Obtener(int CodEmpresa, string usuario)
        {
            return depositosLoteDB.TES_DepositosLote_Ctas_Obtener(CodEmpresa, usuario);
        }

        public ErrorDto<List<TesDepositosTramiteDto>> TES_DepositosLote_ArchivoCarga(int CodEmpresa, string archivoData)
        {
            return depositosLoteDB.TES_DepositosLote_ArchivoCarga(CodEmpresa, archivoData);
        }

        public ErrorDto TES_DepositosLote_Procesar(int CodEmpresa, string cuenta, string usuario, string archivoData)
        {
            return depositosLoteDB.TES_DepositosLote_Procesar(CodEmpresa, cuenta, usuario, archivoData);
        }

        public ErrorDto<TablasListaGenericaModel> TES_DepositosLote_Inconsistencias_Obtener(int CodEmpresa, string filtros)
        {
            return depositosLoteDB.TES_DepositosLote_Inconsistencias_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto<TablasListaGenericaModel> TES_DepositosLote_Registro_Obtener(int CodEmpresa, string filtros)
        {
            return depositosLoteDB.TES_DepositosLote_Registro_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto<string> TES_DepositosLote_CategoriaCta_Obtener(int CodEmpresa, string Categoria)
        {
            return depositosLoteDB.TES_DepositosLote_CategoriaCta_Obtener(CodEmpresa, Categoria);
        }

        public ErrorDto TES_DepositosLote_Registro_Aplicar(int CodEmpresa, string Usuario, string Datos)
        {
            return depositosLoteDB.TES_DepositosLote_Registro_Aplicar(CodEmpresa, Usuario, Datos);
        }

        public ErrorDto TES_DepositosLote_Registro_Actualizar(int CodEmpresa)
        {
            return depositosLoteDB.TES_DepositosLote_Registro_Actualizar(CodEmpresa);
        }

        public ErrorDto TES_DepositosLote_Registro_Desvincular(int CodEmpresa, string Usuario, string Datos)
        {
            return depositosLoteDB.TES_DepositosLote_Registro_Desvincular(CodEmpresa, Usuario, Datos);
        }
    }
}
