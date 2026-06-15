using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo_API.Models.ProGrX.Cajas;
using Galileo_API.DataBaseTier.ProGrX.Cajas;

namespace Galileo_API.BusinessLogic.ProGrX.Cajas
{
    public class FrmCajasAplicacionMultipleBl(
        FrmCajasAplicacionMultipleDb dbFrmCajas_AM)
    {
        private readonly FrmCajasAplicacionMultipleDb DbFrmCajas_AM = dbFrmCajas_AM;

        public FrmCajasAplicacionMultipleBl(IConfiguration config)
            : this(new FrmCajasAplicacionMultipleDb(config))
        {
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_AM_Documentos_Obtener(
            int codEmpresa,
            string codCaja)
        {
            return DbFrmCajas_AM.Cajas_AM_Documentos_Obtener(codEmpresa, codCaja);
        }

        public ErrorDto<CajasAmClienteInicialDto> Cajas_AM_ClienteInicial_Obtener(
            int codEmpresa,
            string cedula)
        {
            return DbFrmCajas_AM.Cajas_AM_ClienteInicial_Obtener(codEmpresa, cedula);
        }

        public ErrorDto<CajasAmValidacionDto> Cajas_AM_Validar(
            int codEmpresa,
            string codCaja,
            int codApertura,
            int sesionId,
            string usuario,
            decimal monto,
            string tiquete)
        {
            return DbFrmCajas_AM.Cajas_AM_Validar(
                codEmpresa, codCaja, codApertura,
                sesionId, usuario, monto, tiquete
            );
        }

        public ErrorDto<List<CajasCreditoPendienteDto>> Cajas_AM_Creditos_Pendientes(int codEmpresa, CajasAMCreditosPendientesRequestDto request)
        {
            return DbFrmCajas_AM.Cajas_AM_Creditos_Pendientes(
                codEmpresa, request
            );
        }

        public ErrorDto<bool> Cajas_AM_Creditos_Agregar(int codEmpresa, List<CajasAmAgregarRequestDto> items)
        {
            return DbFrmCajas_AM.Cajas_AM_Creditos_Agregar(
                codEmpresa, items
            );
        }

        public ErrorDto<bool> Cajas_AM_Eliminar(int codEmpresa,List<long> ids)
        {
            return DbFrmCajas_AM.Cajas_AM_Eliminar(codEmpresa, ids);
        }

        public ErrorDto<long> Cajas_AM_Aplicar(int codEmpresa,CajasAmAplicarRequestDto request)
        {
            return DbFrmCajas_AM.Cajas_AM_Aplicar(
                codEmpresa, request
            );
        }

        public ErrorDto<List<CajasAmSeleccionadoDto>> Cajas_AM_Seleccionados(
            int codEmpresa,
            string cedula,
            string codCaja,
            int codApertura,
            string tiquete)
        {
            return DbFrmCajas_AM.Cajas_AM_Seleccionados(
                codEmpresa, cedula, codCaja, codApertura, tiquete
            );
        }
    }
}
