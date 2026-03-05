using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX.Clientes;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.BusinessLogic.ProGrX.Clientes
{
    public class FrmAfLiquidacionAsientosBL
    {
        private readonly FrmAfLiquidacionAsientosDB _DB;

        public FrmAfLiquidacionAsientosBL(IConfiguration config)
        {
            _DB = new FrmAfLiquidacionAsientosDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_LiqAsientosTipo_Obtener(int CodEmpresa, string accion)
        {
            return _DB.AF_LiqAsientosTipo_Obtener(CodEmpresa, accion);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Af_LiquidacionAsientos_Bancos(
               int CodEmpresa,
               AfLiquidacionFiltroRequest request)
        {
            return _DB.Af_LiquidacionAsientos_Bancos(CodEmpresa, request);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Af_LiquidacionAsientos_Usuarios(
               int CodEmpresa,
               AfLiquidacionFiltroRequest request)
        {
            return _DB.Af_LiquidacionAsientos_Usuarios(CodEmpresa, request);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Af_LiquidacionAsientos_Tokens(
                int CodEmpresa,
                AfLiquidacionFiltroRequest request)
        {
            return _DB.Af_LiquidacionAsientos_Tokens(CodEmpresa, request);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Af_LiquidacionAsientos_Oficinas(
            int CodEmpresa,
            AfLiquidacionFiltroRequest request)
        {
            return _DB.Af_LiquidacionAsientos_Oficinas(CodEmpresa, request);
        }

        public ErrorDto<List<TokenConsultaModel>> AF_LiqAsientosToken_Obtener(int CodEmpresa, string usuario)
        {
            return _DB.AF_LiqAsientosToken_Obtener(CodEmpresa, usuario);
        }

        public ErrorDto AF_LiqAsientoToken_Nuevo(int CodEmpresa, string usuario)
        {
            return _DB.AF_LiqAsientoToken_Nuevo(CodEmpresa, usuario);
        }

        public ErrorDto<AfLiquidacionAsientosGenerarResponse> Af_LiquidacionAsientos_Generar(
             int CodEmpresa,
             AfLiquidacionAsientosGenerarRequest request)
        {
            return _DB.Af_LiquidacionAsientos_Generar(CodEmpresa, request);
        }

        public ErrorDto<List<AfLiquidacionAsientosRowDto>> Af_LiquidacionAsientos_Buscar(
            int CodEmpresa,
            AfLiquidacionAsientosBuscarRequest request)
        {
            return _DB.Af_LiquidacionAsientos_Buscar(CodEmpresa, request);
        }

    }
}