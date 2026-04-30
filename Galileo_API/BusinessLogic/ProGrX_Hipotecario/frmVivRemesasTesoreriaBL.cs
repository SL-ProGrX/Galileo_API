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

        public ErrorDto<int> RemesasTesoreria_Insertar(int codEmpresa, RemesaTesoreriaUpsertDto dto)
            => _db.RemesasTesoreria_Insertar(codEmpresa, dto);

        public ErrorDto<bool> RemesasTesoreria_Actualizar(int codEmpresa, RemesaTesoreriaUpsertDto dto)
            => _db.RemesasTesoreria_Actualizar(codEmpresa, dto);

        public ErrorDto<bool> RemesasTesoreriaDetalle_Eliminar(int codEmpresa, int remesa)
            => _db.RemesasTesoreriaDetalle_Eliminar(codEmpresa, remesa);

    }
}
