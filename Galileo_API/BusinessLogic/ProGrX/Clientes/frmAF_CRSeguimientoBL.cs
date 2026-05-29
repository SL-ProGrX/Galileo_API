using Galileo.DataBaseTier.ProGrX.Clientes;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.BusinessLogic.ProGrX.Clientes
{
    public class FrmAFCrSeguimientoBL
    {
        private readonly FrmAFCrSeguimientoDB _db;

        public FrmAFCrSeguimientoBL(IConfiguration config)
        {
            _db = new FrmAFCrSeguimientoDB(config);
        }

        public ErrorDto<List<AfCrSeguimientoData>> AF_CR_Seguimiento_Obtener(int CodEmpresa, AfCrSeguimientoFiltros filtros)
        {
            return _db.AF_CR_Seguimiento_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_CR_Seguimiento_Obtener_Gestiones(int CodEmpresa)
        {
            return _db.AF_CR_Seguimiento_Obtener_Gestiones(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_CR_Seguimiento_Obtener_Causas(int CodEmpresa)
        {
            return _db.AF_CR_Seguimiento_Obtener_Causas(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_CR_Seguimiento_Obtener_Institucion(int CodEmpresa)
        {
            return _db.AF_CR_Seguimiento_Obtener_Institucion(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_CR_Seguimiento_Obtener_Provincia(int CodEmpresa)
        {
            return _db.AF_CR_Seguimiento_Obtener_Provincia(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_CR_Seguimiento_Obtener_Zona(int CodEmpresa)
        {
            return _db.AF_CR_Seguimiento_Obtener_Zona(CodEmpresa);
        }

        public ErrorDto<AfCrSeguimientoDetalle?> AF_CR_Seguimiento_Obtener_Detalle_Renuncia(int CodEmpresa, int codRenuncia)
        {
            return _db.AF_CR_Seguimiento_Obtener_Detalle_Renuncia(CodEmpresa, codRenuncia);
        }

        public ErrorDto<List<AfCrSeguimientoMotivo>> AF_CR_Seguimiento_Obtener_Motivos(int CodEmpresa, int renunciaId)
        {
            return _db.AF_CR_Seguimiento_Obtener_Motivos(CodEmpresa, renunciaId);
        }

        public ErrorDto<List<AfCrSeguimientoHistorial>> AF_CR_Seguimiento_Obtener_Historial(int CodEmpresa, int codRenuncia)
        {
            return _db.AF_CR_Seguimiento_Obtener_Historial(CodEmpresa, codRenuncia);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_CR_Seguimiento_Obtener_Gestion(int CodEmpresa)
        {
            return _db.AF_CR_Seguimiento_Obtener_Gestion(CodEmpresa);
        }

        public ErrorDto AF_CR_Seguimiento_Motivos_Registrar(int CodEmpresa, AfCrSeguimientoMotivosRegistrar motivos)
        {
            return _db.AF_CR_Seguimiento_Motivos_Registrar(CodEmpresa, motivos);
        }

        public ErrorDto AF_CR_Seguimiento_Renuncia_Estado(int CodEmpresa, AfCrSeguimientoRenunciaEstado estado)
        {
            return _db.AF_CR_Seguimiento_Renuncia_Estado(CodEmpresa, estado);
        }
    }
}