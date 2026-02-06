using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cajas;
using Galileo_API.Models.ProGrX.Cajas;

namespace Galileo_API.BusinessLogic.ProGrX.Cajas
{
    public class FrmCajasIdentificaSfBL
    {
        private readonly FrmCajasIdentificaSfDb _db;

        public FrmCajasIdentificaSfBL(IConfiguration config)
        {
            _db = new FrmCajasIdentificaSfDb(config);
        }

        public ErrorDto Cajas_CajasIdentificaSf_Identificar(
           int codEmpresa,
           string cedula,
           string nombre,
           string usuario,
           string pagadorId,
           string origenRecursosId,
           List<TesDepositoIdentificarDto> casos)
        {
                return _db.Cajas_CajasIdentificaSf_Identificar(
                codEmpresa,
                cedula,
                nombre,
                usuario,
                pagadorId,
                origenRecursosId,
                casos);
        }

        public ErrorDto<List<FrmCajasIdentificaSfDepositoDto>> Cajas_CajasIdentificaSf_Depositos_Obtener(int CodEmpresa)
        {
            return _db.Cajas_CajasIdentificaSf_Depositos_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_CajasIdentificaSf_Entidades_Obtener(int CodEmpresa)
        {
            return _db.Cajas_CajasIdentificaSf_Entidades_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_CajasIdentificaSf_Recursos_Obtener(int CodEmpresa)
        {
            return _db.Cajas_CajasIdentificaSf_Recursos_Obtener(CodEmpresa);
        }

        public ErrorDto<List<FrmCajasIdentificaSfTramitsRsdto>> Cajas_CajasIdentificaSf_Consultar(
                int codEmpresa,
                DateTime fechaInicio,
                DateTime fechaCorte,
                int bancoId,
                decimal montoInicio,
                decimal montoHasta,
                string? numDocumento)
        {
            return _db.Cajas_CajasIdentificaSf_Consultar(
                codEmpresa,
                fechaInicio,
                fechaCorte,
                bancoId,
                montoInicio,
                montoHasta,
                numDocumento);
        }
    }
}
