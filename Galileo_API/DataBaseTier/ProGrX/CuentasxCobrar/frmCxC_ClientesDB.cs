using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using Galileo.Models;
using System.Collections.Generic;

namespace Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar
{
    public class FrmCxCClientesDB
    {
        private readonly PortalDB _portalDb;

        public FrmCxCClientesDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Consulta la lista de personas, ordenadas por el campo indicado.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="orden">Campo de ordenamiento ("Cedula" o "Nombre").</param>
        /// <returns>Lista de personas.</returns>
        public ErrorDto<List<CxcPersonaDto>> CxcPersonas_Lista(int codEmpresa, string orden)
        {
            string orderBy = orden?.ToLower() == "cedula" ? "cedula" : "nombre";
            var query = $@"
                SELECT cedula, nombre
                FROM CxC_Personas
                ORDER BY {orderBy}";
            return DbHelper.ExecuteListQuery<CxcPersonaDto>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Lista de estados civiles activos.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> EstadoCivil_Lista(int codEmpresa)
        {
            var query = @"
                SELECT rtrim(Estado_Civil) as item,
                       rtrim(DESCRIPCION) as descripcion
                FROM SYS_ESTADO_CIVIL
                WHERE ACTIVO = 1";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Lista de clasificaciones de clientes.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> Clasificacion_Lista(int codEmpresa)
        {
            var query = @"
                SELECT rtrim(cod_categoria) as item,
                       rtrim(descripcion) as descripcion
                FROM CxC_Categoria_Clientes";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Lista de tipos de identificación.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> TiposId_Lista(int codEmpresa)
        {
            var query = @"
                SELECT TIPO_ID as item,
                       rtrim(Descripcion) as descripcion
                FROM AFI_TIPOS_IDS";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query);
        }
    }
}
