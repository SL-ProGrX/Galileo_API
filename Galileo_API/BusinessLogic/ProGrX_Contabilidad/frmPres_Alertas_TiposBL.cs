using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.PRES;


namespace Galileo.BusinessLogic
{
    public class FrmPresAlertasTiposBl
    {
        readonly FrmPresAlertasTiposDb _db;

        public FrmPresAlertasTiposBl(IConfiguration config)
        {
            _db = new FrmPresAlertasTiposDb(config);
        }

        public ErrorDto<AlertasTiposLista> AlertasTipos_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            return _db.AlertasTipos_Obtener(CodCliente, pagina, paginacion, filtro);
        }

        public ErrorDto AlertasTipos_Insertar(int CodCliente, AlertasTiposDto request)
        {
            return _db.AlertasTipos_Insertar(CodCliente, request);
        }

        public ErrorDto AlertasTipos_Actualizar(int CodCliente, AlertasTiposDto request)
        {
            return _db.AlertasTipos_Actualizar(CodCliente, request);
        }


        public ErrorDto AlertasTipos_Eliminar(int CodCliente, string tipoalerta)
        {
            return _db.AlertasTipos_Eliminar(CodCliente, tipoalerta);
        }

        public ErrorDto<AlertasTiposJustificacionLista> AlertasTiposJustificacion_Obtener(int CodCliente, string id_justificacion, string? filtro)
    => _db.AlertasTiposJustificacion_Obtener(CodCliente, id_justificacion, filtro);

        public ErrorDto AlertasTiposJustificacion_Guardar(int CodCliente, AlertasTiposJustificacionDto request)
            => _db.AlertasTiposJustificacion_Guardar(CodCliente, request);

        public ErrorDto AlertasTiposJustificacion_Eliminar(int CodCliente, AlertasTiposJustificacionEliminarRequest request)
            => _db.AlertasTiposJustificacion_Eliminar(CodCliente, request);

        public ErrorDto<AlertasTiposDetalleLista> AlertasTiposDetalle_Obtener(int CodCliente, string cod_desviacion, string? filtro)
    => _db.AlertasTiposDetalle_Obtener(CodCliente, cod_desviacion, filtro);

        public ErrorDto AlertasTiposDetalle_Guardar(int CodCliente, AlertasTiposDetalleDto request)
            => _db.AlertasTiposDetalle_Guardar(CodCliente, request);

        public ErrorDto AlertasTiposDetalle_Eliminar(int CodCliente, AlertasTiposDetalleEliminarRequest request)
            => _db.AlertasTiposDetalle_Eliminar(CodCliente, request);
    }
}
