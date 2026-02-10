using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Polizas;
using Galileo_API.Models.ProGrX_Polizas;
using System.Collections.Generic;

namespace Galileo_API.BusinessLogic.ProGrX_Polizas
{
    public class FrmPolizasEstadosReclamosBL
    {
        private readonly FrmPolizasEstadosReclamosDB _db;

        public FrmPolizasEstadosReclamosBL(IConfiguration config)
        {
            _db = new FrmPolizasEstadosReclamosDB(config);
        }

        public ErrorDto<List<PolizasEstadosReclamosDto>> EstadosReclamos_Listar(int codEmpresa)
            => _db.EstadosReclamos_Listar(codEmpresa);

        public ErrorDto<PolizasEstadosReclamosExisteResult?> EstadosReclamos_Existe(int codEmpresa, int idEstado)
            => _db.EstadosReclamos_Existe(codEmpresa, idEstado);

        public ErrorDto<bool> EstadosReclamos_Guardar(int codEmpresa, PolizasEstadosReclamosSaveParams param)
            => _db.EstadosReclamos_Guardar(codEmpresa, param);

        public ErrorDto<bool> EstadosReclamos_Eliminar(int codEmpresa, PolizasEstadosReclamosDeleteParams param)
            => _db.EstadosReclamos_Eliminar(codEmpresa, param);
    }
}
