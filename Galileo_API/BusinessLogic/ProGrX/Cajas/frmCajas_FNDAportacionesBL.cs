using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;

namespace Galileo.BusinessLogic
{
    public class FrmCajasFndaportacionesBL
    {
        private readonly FrmCajasFndaportacionesDB _db;

        public FrmCajasFndaportacionesBL(IConfiguration config)
        {
            _db = new FrmCajasFndaportacionesDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_Documentos_Obtener(int codEmpresa, string codCaja)
        {
            return _db.Cajas_Documentos_Obtener(codEmpresa, codCaja);
        }

        public ErrorDto Fondos_Aporte_Aplicar(int codEmpresa, FondosAporteAplicarDto request)
        {
            return _db.Fondos_Aporte_Aplicar(codEmpresa, request);
        }

        public ErrorDto<FondosRequiereAutorizacionDto> Fondos_Aporte_RequiereAutorizacion(int codEmpresa, string plan, string usuario, decimal aporte)
        {
            return _db.Fondos_Aporte_RequiereAutorizacion(codEmpresa, plan, usuario, aporte);
        }

        public ErrorDto<GestionEstadoDto> Fondos_Gestion_Estado(int codEmpresa, int gestionId)
        {
            return _db.Fondos_Gestion_Estado(codEmpresa, gestionId);
        }

        public ErrorDto<FondosGestionRegistroDto> fondos_gestion_registro(int CodEmpresa, FondosGestionRegistroAddDto request)
        {
            return _db.fondos_gestion_registro(CodEmpresa, request);
        }

        public ErrorDto<FondosContratoDatosDto> Fondos_Contrato_Datos_Obtener(
            int codEmpresa,
            string codCaja,
            int operadora,
            string plan,
            int contrato)
        {
            return _db.Fondos_Contrato_Datos_Obtener(codEmpresa, codCaja, operadora, plan, contrato);
        }

        public ErrorDto<List<FndSubCuentasDto>> SubCuentas_Obtener(int CodEmpresa, string operadora, string plan, int contrato)
        {
            return _db.SubCuentas_Obtener(CodEmpresa, operadora, plan, contrato);
        }
    }
}
