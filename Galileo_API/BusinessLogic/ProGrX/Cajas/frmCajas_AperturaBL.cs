using Galileo.DataBaseTier.ProGrX.Cajas;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;

namespace Galileo.BusinessLogic.ProGrX.Cajas
{
    public class FrmCajasAperturaBl
    {
        private readonly FrmCajasAperturaDb DbCajasApertura;

        public FrmCajasAperturaBl(IConfiguration config)
        {
            DbCajasApertura = new FrmCajasAperturaDb(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_Asignadas_Obtener(int CodEmpresa, string Usuario)
        {
            return DbCajasApertura.Cajas_Asignadas_Obtener(CodEmpresa, Usuario);
        }

        public ErrorDto<List<CajasDivisaDto>> Cajas_Apertura_Divisas_Obtener(int CodEmpresa, int CodConta)
        {
            return DbCajasApertura.Cajas_Apertura_Divisas_Obtener(CodEmpresa, CodConta);
        }

        public ErrorDto<CajaAperturaDetalleDto?> Cajas_Apertura_Detalle_Obtener(int CodEmpresa, string CodCaja)
        {
            return DbCajasApertura.Cajas_Apertura_Detalle_Obtener(CodEmpresa, CodCaja);
        }

        public ErrorDto<List<CajasAperturaTeConsultaData>> Cajas_Apertura_TEConsulta_Obtener(int CodEmpresa, string CodCaja)
        {
            return DbCajasApertura.Cajas_Apertura_TEConsulta_Obtener(CodEmpresa, CodCaja);
        }

        public ErrorDto Cajas_Apertura_UsuarioAutorizado_Validar(int CodEmpresa, string Usuario, string Clave, string CodCaja)
        {
            return DbCajasApertura.Cajas_Apertura_UsuarioAutorizado_Validar(CodEmpresa, Usuario, Clave, CodCaja);
        }

        public ErrorDto<CajaAperturaResponseDto> Cajas_Apertura_Aplicar(int CodEmpresa, CajaAperturaRequestDto req)
        {
            return DbCajasApertura.Cajas_Apertura_Aplicar(CodEmpresa, req);
        }
    }
}
