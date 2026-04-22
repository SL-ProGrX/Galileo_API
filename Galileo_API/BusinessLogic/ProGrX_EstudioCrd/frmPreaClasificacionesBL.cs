using Galileo_API.DataBaseTier.ProGrX_EstudioCrd;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;

namespace Galileo_API.BusinessLogic.ProGrX_EstudioCrd
{
    public class FrmPreaClasificacionesBl
    {
        private readonly FrmPreaClasificacionesDb _db;

        public FrmPreaClasificacionesBl(IConfiguration config)
            => _db = new FrmPreaClasificacionesDb(config);

        public ErrorDto<List<PreaClasificacionRazonData>> PreaClasificacion_Razones_Obtener(int codEmpresa)
        {
            return _db.PreaClasificacion_Razones_Obtener(codEmpresa);
        }

        public ErrorDto<List<PreaClasificacionData>> PreaClasificacion_Catalogo_Obtener(int codEmpresa, string catalogo)
        {
            return _db.PreaClasificacion_Catalogo_Obtener(codEmpresa, catalogo);
        }

        public ErrorDto PreaClasificacion_Razon_Guardar(int codEmpresa, string usuario, PreaClasificacionRazonData request)
        {
            return _db.PreaClasificacion_Razon_Guardar(codEmpresa, usuario, request);
        }

        public ErrorDto PreaClasificacion_Razon_Eliminar(int codEmpresa, string codRazon, string usuario)
        {
            return _db.PreaClasificacion_Razon_Eliminar(codEmpresa, codRazon, usuario);
        }

        public ErrorDto PreaClasificacion_Garantia_Guardar(int codEmpresa, string usuario, PreaClasificacionData request)
        {
            return _db.PreaClasificacion_Garantia_Guardar(codEmpresa, usuario, request);
        }

        public ErrorDto PreaClasificacion_Garantia_Eliminar(int codEmpresa, string codGarantia, string usuario)
        {
            return _db.PreaClasificacion_Garantia_Eliminar(codEmpresa, codGarantia, usuario);
        }

        public ErrorDto PreaClasificacion_Mora_Guardar(int codEmpresa, string usuario, PreaClasificacionData request)
        {
            return _db.PreaClasificacion_Mora_Guardar(codEmpresa, usuario, request);
        }

        public ErrorDto PreaClasificacion_Mora_Eliminar(int codEmpresa, string codMora, string usuario)
        {
            return _db.PreaClasificacion_Mora_Eliminar(codEmpresa, codMora, usuario);
        }

        public ErrorDto PreaClasificacion_Capacidad_Guardar(int codEmpresa, string usuario, PreaClasificacionData request)
        {
            return _db.PreaClasificacion_Capacidad_Guardar(codEmpresa, usuario, request);
        }

        public ErrorDto PreaClasificacion_Capacidad_Eliminar(int codEmpresa, string codCapacidad, string usuario)
        {
            return _db.PreaClasificacion_Capacidad_Eliminar(codEmpresa, codCapacidad, usuario);
        }

        public ErrorDto PreaClasificacion_Endeudamiento_Guardar(int codEmpresa, string usuario, PreaClasificacionData request)
        {
            return _db.PreaClasificacion_Endeudamiento_Guardar(codEmpresa, usuario, request);
        }

        public ErrorDto PreaClasificacion_Endeudamiento_Eliminar(int codEmpresa, string codEndeudamiento, string usuario)
        {
            return _db.PreaClasificacion_Endeudamiento_Eliminar(codEmpresa, codEndeudamiento, usuario);
        }

        public ErrorDto PreaClasificacion_Historial_Guardar(int codEmpresa, string usuario, PreaClasificacionData request)
        {
            return _db.PreaClasificacion_Historial_Guardar(codEmpresa, usuario, request);
        }

        public ErrorDto PreaClasificacion_Historial_Eliminar(int codEmpresa, string codHistorial, string usuario)
        {
            return _db.PreaClasificacion_Historial_Eliminar(codEmpresa, codHistorial, usuario);
        }
    }
}
