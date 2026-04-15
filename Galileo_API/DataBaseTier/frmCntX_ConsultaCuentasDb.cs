using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class FrmCntXConsultaCuentasDb
    {
        private readonly PortalDB _portalDB;
        private const string Icon = "pi pi-fw pi-folder";
        private const string ExpandedIcon = "pi pi-fw pi-folder-open";
        private const string CollapsedIcon = "pi pi-fw pi-folder";

        public FrmCntXConsultaCuentasDb(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        public List<CtnxCuentasDto> ObtenerCuentas(int codEmpresa, CuentaVarModel cuenta)
        {
            try
            {
                using var connection = DbHelper.OpenConnection(_portalDB, codEmpresa);
                if (cuenta.Cuenta == "T") // Tipo de Cuenta
                {
                    const string sql = @"
                        SELECT cod_cuenta, cod_cuenta_Mask, descripcion, acepta_movimientos, COD_DIVISA
                        FROM CntX_Cuentas
                        WHERE cuenta_madre = ''
                          AND cod_contabilidad = @Contabilidad
                          AND TIPO_CUENTA = @TipoCuenta
                        ORDER BY cod_cuenta;";

                    return connection.Query<CtnxCuentasDto>(sql, new
                    {
                        Contabilidad = cuenta.Contabilidad,
                        TipoCuenta = cuenta.Cuenta
                    }).ToList();
                }

                const string procedure = "spCntX_Consulta_Cuentas";
                return connection.Query<CtnxCuentasDto>(procedure, new
                {
                    Contabilidad = cuenta.Contabilidad,
                    Cuenta = cuenta.Cuenta,
                    Descripcion = cuenta.Descripcion,
                    Divisa = cuenta.Divisa,
                    Nivel = cuenta.Nivel
                }, commandType: CommandType.StoredProcedure).ToList();
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                return new List<CtnxCuentasDto>();
            }
        }
        
        public List<CtnxCuentasArbolModel> ObtenerCuentasArbol(int codEmpresa, CuentaVarModel cuenta)
        {
            try
            {
                using var connection = DbHelper.OpenConnection(_portalDB, codEmpresa);
                const string sql = @"
                    SELECT cod_cuenta, cuenta_madre, cod_cuenta_Mask, descripcion, acepta_movimientos
                    FROM CntX_Cuentas
                    WHERE cod_contabilidad = @Contabilidad
                    ORDER BY cod_cuenta;";

                var info = connection.Query<CtnxCuentasDto>(sql, new
                {
                    Contabilidad = cuenta.Contabilidad
                }).ToList();

                var resp = new List<CtnxCuentasArbolModel>();

                foreach (var item in info.Where(i => string.IsNullOrWhiteSpace(i.cuenta_madre)))
                {
                    var children = AddCuentasArbol(info, item);

                    resp.Add(new CtnxCuentasArbolModel
                    {
                        Key = item.cod_cuenta,
                        Label = $"{item.cod_cuenta_Mask}-{item.descripcion}",
                        Data = item.cod_cuenta_Mask,
                        Icon = Icon,
                        ExpandedIcon = ExpandedIcon,
                        CollapsedIcon = CollapsedIcon,
                        Children = children,
                        leaf = children.Count == 0
                    });
                }

                return resp;
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                return new List<CtnxCuentasArbolModel>();
            }
        }

        public List<DropDownListaGenericaModel> ObtenerDivisas(int codEmpresa, int contabilidad)
        {
            try
            {
                using var connection = DbHelper.OpenConnection(_portalDB, codEmpresa);
                const string sql = @"
                    SELECT RTRIM(cod_divisa) AS item, RTRIM(descripcion) AS descripcion
                    FROM CntX_Divisas
                    WHERE cod_contabilidad = @Contabilidad
                    ORDER BY divisa_local DESC;";

                return connection.Query<DropDownListaGenericaModel>(sql, new
                {
                    Contabilidad = contabilidad
                }).ToList();
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                return new List<DropDownListaGenericaModel>();
            }
        }

        public List<DropDownListaGenericaModel> ObtenerTiposCuentas(int codEmpresa, int contabilidad)
        {
            
            try
            {
                using var connection = DbHelper.OpenConnection(_portalDB, codEmpresa);
                const string sql = @"
                    SELECT TIPO_CUENTA AS item, Descripcion AS descripcion
                    FROM CntX_Tipos_Cuentas
                    WHERE cod_contabilidad = @Contabilidad
                    ORDER BY Prioridad, Tipo_cuenta;";

                return connection.Query<DropDownListaGenericaModel>(sql, new
                {
                    Contabilidad = contabilidad
                }).ToList();
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                return new List<DropDownListaGenericaModel>();
            }
        }

        private static List<CtnxCuentasArbolModel> AddCuentasArbol(List<CtnxCuentasDto> cuentas, CtnxCuentasDto cuenta)
        {
            var resp = new List<CtnxCuentasArbolModel>();
            foreach (var item in cuentas)
            {
                if (item.cuenta_madre == cuenta.cod_cuenta)
                {
                    var children = AddCuentasArbol(cuentas, item);
                    resp.Add(new CtnxCuentasArbolModel
                    {
                        Key = item.cod_cuenta,
                        Label = $"{item.cod_cuenta_Mask}-{item.descripcion}",
                        Data = item.cod_cuenta_Mask,
                        Icon = Icon,
                        ExpandedIcon = ExpandedIcon,
                        CollapsedIcon = CollapsedIcon,
                        Children = children,
                        leaf = children.Count == 0
                    });
                }
            }

            return resp;
        }
    }
}