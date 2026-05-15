using Dapper;
using System.Data;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXErEspecialDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _mSecurityMain;
        private const int vModulo = 20;

        public FrmCntXErEspecialDb(IConfiguration config)
            : this(
                  new PortalDB(config),
                  new MSecurityMainDb(config))
        {
        }

        public FrmCntXErEspecialDb(
            PortalDB portalDb,
            MSecurityMainDb mSecurityMain)
        {
            _portalDb = portalDb;
            _mSecurityMain = mSecurityMain;
        }

        /// <summary>
        /// Obtiene un ER especial por codigo.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="codErEspecial"></param>
        /// <returns></returns>
        public ErrorDto<CntXErEspecialDefinicionData?> CntX_EREspecial_Consulta_Obtener(
            int codEmpresa,
            int codContabilidad,
            int codErEspecial)
        {
            const string query = @"
                select top 1
                    cod_er_especial,
                    isnull(descripcion, '') as descripcion,
                    isnull(titulo, '') as titulo,
                    'US-CREA: ' + isnull(REGISTRO_USUARIO, '') + ' FECHA-CREA: ' +
                    isnull(convert(varchar(10), REGISTRO_FECHA, 111), '') as detalle 
                from CNTX_ER_ESPECIAL
                where cod_contabilidad = @codContabilidad
                  and cod_er_especial = @codErEspecial;";

            return DbHelper.ExecuteSingleQuery<CntXErEspecialDefinicionData>(
                _portalDb,
                codEmpresa,
                query,
                null,
                new { codContabilidad, codErEspecial });
        }

        /// <summary>
        /// Obtiene la lista de ER especiales.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <returns></returns>
        public ErrorDto<List<CntXErEspecialDefinicionData>> CntX_EREspecial_Lista_Obtener(
            int codEmpresa,
            int codContabilidad)
        {
            const string query = @"
                select cod_er_especial, descripcion, titulo 
                from CNTX_ER_ESPECIAL
                where cod_contabilidad = @codContabilidad
                order by cod_er_especial;";

            return DbHelper.ExecuteListQuery<CntXErEspecialDefinicionData>(
                _portalDb,
                codEmpresa,
                query,
                new { codContabilidad });
        }

        /// <summary>
        /// Guarda el encabezado del ER especial.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CntX_EREspecial_Guardar(
            int codEmpresa, int codContabilidad, string usuario, CntXErEspecialDefinicionData request)
        {
            var localConn = _portalDb.ObtenerDbConnStringEmpresa(codEmpresa);

            string user = (usuario ?? string.Empty).Trim().ToUpper();
            try
            {
                using var connection = new SqlConnection(localConn);
                connection.Open();

                var codErEspecial = request.cod_er_especial;

                if (codErEspecial > 0)
                {
                    connection.Execute(
                        @"
                        update CNTX_ER_ESPECIAL
                        set descripcion = @descripcion,
                            titulo = @titulo
                        where cod_contabilidad = @codContabilidad
                          and cod_er_especial = @codErEspecial;",
                        new
                        {
                            descripcion = request.descripcion.Trim().ToUpper(),
                            titulo = request.titulo.Trim(),
                            codContabilidad,
                            codErEspecial
                        });

                    RegistrarBitacora(
                        codEmpresa,
                        user,
                        "Modifica - WEB",
                        $"ER ESPECIAL: {codErEspecial} EMP: {codContabilidad}");
                }
                else
                {
                    using var transaction = connection.BeginTransaction(IsolationLevel.Serializable);

                    codErEspecial = connection.ExecuteScalar<int>(
                        @"
                        select isnull(max(cod_er_especial), 0) + 1
                        from CNTX_ER_ESPECIAL with (updlock, holdlock)
                        where cod_contabilidad = @codContabilidad;",
                                        new { codContabilidad },
                                        transaction);

                                    connection.Execute(
                                        @"
                        insert into CNTX_ER_ESPECIAL
                        (
                            cod_contabilidad,
                            cod_er_especial,
                            descripcion,
                            titulo,
                            REGISTRO_USUARIO,
                            REGISTRO_FECHA
                        )
                        values
                        (
                            @codContabilidad,
                            @codErEspecial,
                            @descripcion,
                            @titulo,
                            @usuario,
                            getdate()
                        );",
                        new
                        {
                            codContabilidad,
                            codErEspecial,
                            descripcion = request.descripcion.Trim().ToUpper(),
                            titulo = request.titulo.Trim(),
                            usuario = user
                        },
                        transaction);

                    transaction.Commit();

                    RegistrarBitacora(
                        codEmpresa,
                        user,
                        "Registra - WEB",
                        $"ER ESPECIAL: {codErEspecial} EMP: {codContabilidad}");
                }

                return new ErrorDto
                {
                    Code = codErEspecial,
                    Description = "Informacion guardada satisfactoriamente..."
                };
            }
            catch (Exception ex)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = ex.Message
                };
            }
        }

        /// <summary>
        /// Borra un ER especial y sus cuentas relacionadas.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="codErEspecial"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CntX_EREspecial_Borrar(
            int codEmpresa, int codContabilidad, int codErEspecial, string usuario)
        {
            const string query = @"
                delete CNTX_ER_ESPECIAL_DETALLE
                where cod_contabilidad = @codContabilidad
                  and cod_er_especial = @codErEspecial;

                delete CNTX_ER_ESPECIAL
                where cod_contabilidad = @codContabilidad
                  and cod_er_especial = @codErEspecial;";

            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                query,
                new { codContabilidad, codErEspecial });

            if (resp.Code == -1)
            {
                return resp;
            }

            RegistrarBitacora(
                codEmpresa,
                usuario,
                "Elimina - WEB",
                $"ER ESPECIAL: {codErEspecial} EMP: {codContabilidad}");

            return new ErrorDto
            {
                Code = 0,
                Description = "Registro eliminado satisfactoriamente."
            };
        }

        /// <summary>
        /// Obtiene el arbol inicial de cuentas con seleccion por grupo y accion.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<CntXErEspecialCuentaNodeData>> CntX_EREspecial_Arbol_Obtener(
            int codEmpresa,
            int codContabilidad,
            CntXErEspecialArbolRequest request)
        {
            var localConn = _portalDb.ObtenerDbConnStringEmpresa(codEmpresa);

            try
            {
                using var connection = new SqlConnection(localConn);
                connection.Open();

                var tipos = connection.Query<CntXErEspecialTipoCuentaData>(
                    @"
                    select
                        tipo_cuenta,
                        descripcion
                    from CntX_Tipos_Cuentas
                    where cod_contabilidad = @codContabilidad
                    order by tipo_cuenta;",
                    new { codContabilidad }).ToList();

                var cuentas = connection.Query<CntXErEspecialCuentaData>(
                    @"
                    select
                        cod_cuenta,
                        descripcion,
                        isnull(cuenta_madre, '') as cuenta_madre,
                        tipo_cuenta
                    from CntX_Cuentas
                    where cod_contabilidad = @codContabilidad
                    order by cod_cuenta;",
                    new { codContabilidad }).ToList();

                var marcadas = request.cod_er_especial > 0
                    ? connection.Query<CntXErEspecialCuentaMarcadaData>(
                        @"
                        select cod_cuenta
                        from CNTX_ER_ESPECIAL_DETALLE
                        where cod_contabilidad = @codContabilidad
                          and cod_er_especial = @codErEspecial
                          and bloque = @bloque
                          and operacion = @operacion;",
                        new
                        {
                            codContabilidad,
                            codErEspecial = request.cod_er_especial,
                            bloque = request.bloque,
                            operacion = request.operacion
                        }).Select(x => x.cod_cuenta).ToHashSet()
                    : new HashSet<string>();

                var root = new CntXErEspecialCuentaNodeData
                {
                    key = "CntX_Cuentas",
                    label = "CntX_Cuentas",
                    tipo = "root",
                    loaded = true
                };

                foreach (var tipo in tipos)
                {
                    var tipoNode = new CntXErEspecialCuentaNodeData
                    {
                        key = $"0x0{tipo.tipo_cuenta}T",
                        label = tipo.descripcion,
                        tipo = "tipo",
                        codigo = tipo.tipo_cuenta,
                        loaded = true
                    };

                    var cuentasRaiz = cuentas
                        .Where(x =>
                            x.tipo_cuenta == tipo.tipo_cuenta &&
                            string.IsNullOrWhiteSpace(x.cuenta_madre))
                        .ToList();

                    tipoNode.children = ConstruirNodosCuenta(
                        cuentas,
                        cuentasRaiz,
                        marcadas);

                    root.children.Add(tipoNode);
                }

                return new ErrorDto<List<CntXErEspecialCuentaNodeData>>
                {
                    Code = 0,
                    Description = string.Empty,
                    Result = new List<CntXErEspecialCuentaNodeData> { root }
                };
            }
            catch (Exception ex)
            {
                return new ErrorDto<List<CntXErEspecialCuentaNodeData>>
                {
                    Code = -1,
                    Description = ex.Message,
                    Result = new List<CntXErEspecialCuentaNodeData>()
                };
            }
        }

        /// <summary>
        /// Guarda la seleccion de cuentas de un grupo y accion del ER especial.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CntX_EREspecial_Cuentas_Guardar(
            int codEmpresa,
            int codContabilidad,
            CntXErEspecialCuentasGuardarRequest request)
        {
            if (request.cod_er_especial <= 0)
            {
                return new ErrorDto
                {
                    Code = -2,
                    Description = "Debe guardar primero el ER especial."
                };
            }

            var localConn = _portalDb.ObtenerDbConnStringEmpresa(codEmpresa);

            try
            {
                using var connection = new SqlConnection(localConn);
                connection.Open();

                using var transaction = connection.BeginTransaction();

                connection.Execute(
                    @"
                    delete CNTX_ER_ESPECIAL_DETALLE
                    where cod_contabilidad = @codContabilidad
                      and cod_er_especial = @codErEspecial
                      and bloque = @bloque
                      and operacion = @operacion;",
                    new
                    {
                        codContabilidad,
                        codErEspecial = request.cod_er_especial,
                        bloque = request.bloque,
                        operacion = request.operacion
                    },
                    transaction);

                var cuentasSeleccionadas = request.cuentas
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim())
                    .Distinct()
                    .ToList();

                foreach (var cuenta in cuentasSeleccionadas)
                {
                    connection.Execute(
                        @"
                        insert into CNTX_ER_ESPECIAL_DETALLE
                        (
                            cod_contabilidad,
                            cod_er_especial,
                            cod_cuenta,
                            bloque,
                            operacion
                        )
                        values
                        (
                            @codContabilidad,
                            @codErEspecial,
                            @codCuenta,
                            @bloque,
                            @operacion
                        );",
                        new
                        {
                            codContabilidad,
                            codErEspecial = request.cod_er_especial,
                            codCuenta = cuenta,
                            bloque = request.bloque,
                            operacion = request.operacion
                        },
                        transaction);
                }

                transaction.Commit();

                return new ErrorDto
                {
                    Code = 0,
                    Description = "Grupo actualizado satisfactoriamente."
                };
            }
            catch (Exception ex)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = ex.Message
                };
            }
        }

        private List<CntXErEspecialCuentaNodeData> ConstruirNodosCuenta(
            List<CntXErEspecialCuentaData> todas,
            List<CntXErEspecialCuentaData> actuales,
            HashSet<string> marcadas)
        {
            var result = new List<CntXErEspecialCuentaNodeData>();

            foreach (var cuenta in actuales)
            {
                var codigo = cuenta.cod_cuenta?.Trim() ?? string.Empty;

                var node = new CntXErEspecialCuentaNodeData
                {
                    key = $"0x0{codigo}C",
                    label = $"{codigo} - {cuenta.descripcion}",
                    tipo = "cuenta",
                    codigo = codigo,
                    @checked = marcadas.Contains(codigo),
                    loaded = true
                };

                var hijas = todas
                    .Where(x => (x.cuenta_madre ?? string.Empty).Trim() == codigo)
                    .ToList();

                node.children = ConstruirNodosCuenta(todas, hijas, marcadas);
                result.Add(node);
            }

            return result;
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, string movimiento, string detalle)
        {
            _mSecurityMain.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = (usuario ?? string.Empty).Trim().ToUpper(),
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }
    }
}