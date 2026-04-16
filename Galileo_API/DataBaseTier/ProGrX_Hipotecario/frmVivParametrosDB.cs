using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Hipotecario;

namespace Galileo_API.DataBaseTier.ProGrX_Hipotecario
{
    public class FrmVivParametrosDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _bitacora;
        private readonly int vModulo = 3;

        public FrmVivParametrosDb(IConfiguration config)
           : this(
                 new PortalDB(config),
                 new MSecurityMainDb(config))
        {
        }

        public FrmVivParametrosDb(PortalDB portalDB, MSecurityMainDb dbBitacora)
        {
            _portalDb = portalDB;
            _bitacora = dbBitacora;
        }

        /// <summary>
        /// Obtiene los parametros
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<VivParametrosData>> VivParametros_Obtener(int codEmpresa)
        {
            const string querySP = @"exec spCRDVivParametros";
            DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, querySP);

            const string query = @"select codigoParametro,descripcion,valor 
                from ViviendaParametros order by codigoParametro";
            return DbHelper.ExecuteListQuery<VivParametrosData>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Obtiene los tipos de desembolsos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> VivTiposDesembolsos_Obtener(int codEmpresa)
        {
            const string query = @"SELECT Codigo as item, Descripcion FROM ViviendaTiposDesembolsos";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Guarda el valor del parametro
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto VivParametros_Guardar(int codEmpresa, string usuario, VivParametrosData request)
        {
            const string sql = @"update ViviendaParametros set valor = @Valor where CodigoParametro = @Codigo";

            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    Valor = request.valor,
                    Codigo = request.codigoParametro,
                });

            if (resp.Code < 0)
                return resp;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: "Modifica - WEB",
                detalle: $"Vivienda tipo desembolso: {request.codigoParametro}"
            );

            return resp;
        }

        /// <summary>
        /// Registra en bitacora
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="movimiento"></param>
        /// <param name="detalle"></param>
        private void RegistrarBitacora(int codEmpresa, string usuario, string movimiento, string detalle)
        {
            _bitacora.Bitacora(new Galileo.Models.Security.BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }
    }
}
