using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Cajas;
using Galileo_API.DataBaseTier.ProGrX.Cajas;

namespace Galileo_API.BusinessLogic.ProGrX.Cajas
{
    public class FrmCajasDatosAsociadosBl(FrmCajasDatosAsociadosDb dbfrmCajas_DatosAsociados)
    {
        public FrmCajasDatosAsociadosBl(IConfiguration config)
            : this(new FrmCajasDatosAsociadosDb(config))
        { }

        public ErrorDto<List<CajasCreditoDto>> Cajas_Consulta_Creditos(int codEmpresa, string cedula)
        {
            return dbfrmCajas_DatosAsociados.Cajas_Consulta_Creditos(codEmpresa, cedula);
        }

        public ErrorDto<List<CajasFondosDto>> Cajas_Consulta_Fondos(int codEmpresa, string cedula, string usuario)
        {
            return dbfrmCajas_DatosAsociados.Cajas_Consulta_Fondos(codEmpresa, cedula, usuario);
        }

        public ErrorDto<List<CajasCxcDto>> Cajas_Consulta_CxC(int codEmpresa, string cedula)
        {
            return dbfrmCajas_DatosAsociados.Cajas_Consulta_CxC(codEmpresa, cedula);
        }

        public ErrorDto<List<CajasServiciosDto>> Cajas_Consulta_Servicios(int codEmpresa, string cedula)
        {
            return dbfrmCajas_DatosAsociados.Cajas_Consulta_Servicios(codEmpresa, cedula);
        }

        public ErrorDto<List<CajasSaldoFavorDto>> Cajas_Consulta_SaldosFavor(int codEmpresa, string cedula, bool liquidados)
        {
            return dbfrmCajas_DatosAsociados.Cajas_Consulta_SaldosFavor(codEmpresa, cedula, liquidados);
        }

        public ErrorDto<List<CajasReciboMultipleDto>> Cajas_Consulta_RecibosMultiples(int codEmpresa, string cedula)
        {
            return dbfrmCajas_DatosAsociados.Cajas_Consulta_RecibosMultiples(codEmpresa, cedula);
        }

        public ErrorDto<CajasDatosPersonaDto?> Cajas_DatosPersona_Operacion_Obtener(int codEmpresa, int operacion)
        {
            return dbfrmCajas_DatosAsociados.Cajas_DatosPersona_Operacion_Obtener(codEmpresa, operacion);
        }

        public ErrorDto<CajasDatosPersonaDto> Cajas_DatosPersona_Validar(int codEmpresa, string cedula, string usuario)
        {
            return dbfrmCajas_DatosAsociados.Cajas_DatosPersona_Validar(codEmpresa, cedula, usuario);
        }
    }
}
