using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Hipotecario;
using Galileo_API.Models.ProGrX_Hipotecario;
using System.Collections.Generic;

namespace Galileo_API.BusinessLogic.ProGrX_Hipotecario
{
    public class FrmVivRemesasTesoreriaBL
    {
        private readonly FrmVivRemesasTesoreriaDB _db;

        public FrmVivRemesasTesoreriaBL(IConfiguration config)
        {
            _db = new FrmVivRemesasTesoreriaDB(config);
        }

        public ErrorDto<List<RemesasTesoreriaObtenerDto>> RemesasTesoreria_Obtener(int codEmpresa)
            => _db.RemesasTesoreria_Obtener(codEmpresa);
    }
}
