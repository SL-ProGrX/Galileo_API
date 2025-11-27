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
    }
}
