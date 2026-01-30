using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.DataBaseTier.ProGrX_Polizas;
using Galileo_API.Models.ProGrX_Polizas;
using Microsoft.Data.SqlClient;

namespace Galileo_API.BusinessLogic.ProGrX_Polizas
{
    public class FrmCRPolizasAseguradorasBl(FrmCRPolizasAseguradorasDb dbfrmCR_PolizasAseguradoras)
    {
        private readonly FrmCRPolizasAseguradorasDb DbfrmCR_PolizasAseguradorasDb = dbfrmCR_PolizasAseguradoras;

        public FrmCRPolizasAseguradorasBl(IConfiguration config)
            : this(new FrmCRPolizasAseguradorasDb(config))
        {
        }

        public ErrorDto<PolizaAseguradoraDto?> Consultar(int codEmpresa,string codigo)
        {
            return DbfrmCR_PolizasAseguradorasDb.Consultar(codEmpresa, codigo);
        }

        public ErrorDto<int> Insertar(int codEmpresa, PolizaAseguradoraDto m)
        {
            return DbfrmCR_PolizasAseguradorasDb.Insertar(codEmpresa, m);
        }

        public ErrorDto<int> Actualizar(int codEmpresa, PolizaAseguradoraDto m)
        {
            return DbfrmCR_PolizasAseguradorasDb.Actualizar(codEmpresa, m);
        }

        public ErrorDto<int> Borrar(int codEmpresa, string codigo)
        {
            return DbfrmCR_PolizasAseguradorasDb.Borrar(codEmpresa, codigo);
        }

        public ErrorDto<string?> Scroll(int codEmpresa,string codigoActual,int direccion)
        {
            return DbfrmCR_PolizasAseguradorasDb.Scroll(codEmpresa, codigoActual, direccion);
        }

        public ErrorDto<List<CuentaBancariaDto>> CuentasBancarias(int codEmpresa,string cedula)
        {
            return DbfrmCR_PolizasAseguradorasDb.CuentasBancarias(codEmpresa, cedula);
        }
        public ErrorDto<List<ProvinciaaseguradoraDto>> ObtenerProvincias(int codEmpresa)
        {
            return DbfrmCR_PolizasAseguradorasDb.ObtenerProvincias(codEmpresa);
        }

        public ErrorDto<List<CantonaseguradoraDto>> ObtenerCantones(int codEmpresa,string provincia)
        {
            return DbfrmCR_PolizasAseguradorasDb.ObtenerCantones(codEmpresa, provincia);
        }

        public ErrorDto<List<DistritoaseguradoraDto>> ObtenerDistritos(int codEmpresa,string provincia,string canton)
        {
            return DbfrmCR_PolizasAseguradorasDb.ObtenerDistritos(codEmpresa, provincia, canton);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Listar(int codEmpresa)
        {
            return DbfrmCR_PolizasAseguradorasDb.Listar(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> ObtenerBancos(int codEmpresa, string usuario)
        {
            return DbfrmCR_PolizasAseguradorasDb.ObtenerBancos(codEmpresa, usuario);
        }




    }
}