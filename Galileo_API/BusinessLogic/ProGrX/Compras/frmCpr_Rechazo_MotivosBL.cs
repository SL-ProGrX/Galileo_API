using Galileo.DataBaseTier;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic
{
    public class FrmCprRechazoMotivosBL
    {
        readonly FrmCprRechazoMotivosDB _db;

        public FrmCprRechazoMotivosBL(IConfiguration config)
        {
            _db = new FrmCprRechazoMotivosDB(config);
        }

        public ErrorDto<CprRechazosMotivosLista> CprRechazoMotivoLista_Obtener(int CodCliente, string vFiltros)
        {
            return _db.CprRechazoMotivoLista_Obtener(CodCliente, vFiltros);
        }

        public ErrorDto CprRechazoMotivo_Guardar(int CodCliente, CprRechazosMotivosDto motivo)
        {
            return _db.CprRechazoMotivo_Guardar(CodCliente, motivo);
        }

        public ErrorDto CprRechazoMotivo_Eliminar(int CodCliente, string cod_rechazo)
        {
            return _db.cprRechazoMotivo_Eliminar(CodCliente, cod_rechazo);
        }


    }
}