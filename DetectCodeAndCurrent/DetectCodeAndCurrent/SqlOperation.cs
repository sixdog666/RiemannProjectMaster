using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Data;


namespace DetectCodeAndCurrent {
    public static class SqlOperation {

        /// <summary>
        /// 获取工位信息，
        /// </summary>
        /// <param name="station"></param>
        /// <returns></returns>
        public static DataTable GetStationInfo(int station) {
            string sql = "SELECT * FROM jixing_db.tbl_stationstatu WHERE Station = @station;";
            MySqlParameter paramStation = new MySqlParameter();
            MysqlConnector instance = MysqlConnector.GetInstance();
            paramStation.ParameterName = "station";
            paramStation.Value = station;
            MySqlParameter[] parameters = new MySqlParameter[] { paramStation };
            DataTable dt = instance.GetMySqlRead(sql, parameters);
            return dt;
        }
        /// <summary>
        /// 获取记录信息
        /// </summary>
        /// <param name="productCode"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public static DataTable GetRecordFormSQL(string productCode, DateTime startTime, DateTime endTime) {
            string sql;
            MySqlParameter[] parameters;
            MySqlParameter paramProductCode = new MySqlParameter();
            MySqlParameter paramStartTime = new MySqlParameter();
            MySqlParameter paramEndTime = new MySqlParameter();
            paramProductCode.ParameterName = "productCode";
            paramProductCode.Value = productCode;
            paramStartTime.ParameterName = "startTime";
            paramStartTime.Value = startTime;
            paramEndTime.ParameterName = "endTime";
            paramEndTime.Value = endTime;
            MysqlConnector instance = MysqlConnector.GetInstance();
            parameters = new MySqlParameter[] { paramProductCode, paramStartTime, paramEndTime };
            if (productCode != "") {
                sql = "SELECT * FROM jixing_db.record_view Where 总装条形码=@productCode and 日期 > @startTime and 日期 < @endTime order by 日期 desc;"; //sql语句
            }
            else {
                sql = "SELECT * FROM jixing_db.record_view Where  日期 > @startTime and 日期 < @endTime order by 日期 desc;";
            }
            return instance.GetMySqlRead(sql, parameters);
        }

        public static void UpdateButtonState(string currentProductCode,string buttonName,bool Value) {

        }

        public static string GetButtonState(string buttonColName,string currentProductCode) {
            MySqlParameter[] parameters;

            parameters = new MySqlParameter[] { new MySqlParameter() { ParameterName = "buttonColName" , Value = buttonColName },
                                                new MySqlParameter() {  ParameterName = "currentProductCode" , Value = currentProductCode } };
            string dbString = "select "+ buttonColName + " from tbl_resultrecord where productCode = @currentProductCode";
            MysqlConnector instance = MysqlConnector.GetInstance();
            DataTable dt= instance.GetMySqlRead(dbString, parameters);
            if (dt != null && dt.Rows.Count > 0 && dt.Rows[0][0].ToString()!=string.Empty) {
                return dt.Rows[0][0].ToString();
            }
            return string.Empty;
        }

        public static DataTable GetButtonConfigInfo() {
            string dbString = "SELECT tbl_ButtonName as 检测项,tbl_MaxValue as 最大值, tbl_MinValue as 最小值, tbl_TestValue as 测量值, tbl_State as 当前状态 ,tbl_ColumnName as ID FROM jixing_db.tbl_infoteg ORDER BY idx";
            MysqlConnector instance = MysqlConnector.GetInstance();
            return instance.GetMySqlRead(dbString);
        }

        public static DataRow GetButtonInfo(string strName,out string nextTestName) {
            MySqlParameter[] parameters;

            parameters = new MySqlParameter[] {
                new MySqlParameter { ParameterName ="strName",Value = strName}};
            string dbString = "select * from tbl_infoteg where tbl_ButtonName = @strName";
            MysqlConnector instance = MysqlConnector.GetInstance();
            DataTable dt = instance.GetMySqlRead(dbString, parameters);
            if (dt != null && dt.Rows.Count > 0) {
                DataRow dr = dt.Rows[0];
                parameters = new MySqlParameter[] {
                    new MySqlParameter {ParameterName ="index",Value = (int)dr["idx"]+1 }
                };
                dbString = "select tbl_ButtonName from tbl_infoteg where idx = @index";
                DataTable next = instance.GetMySqlRead(dbString, parameters);
                if (next != null && next.Rows.Count > 0)
                    nextTestName = next.Rows[0][0].ToString();
                else nextTestName = string.Empty;
                return dr;
            }
            else {
                nextTestName = string.Empty;
                return null;
            }
        }




        /// <summary>
        /// 获取产品条码对应的零件记录
        /// </summary>
        /// <param name="productCodeBar"></param>
        /// <param name="currentProductCode"></param>
        /// <param name="productInfosList"></param>
        public static void GetCurrentPartRecordFromSQL(string productCodeBar, string currentProductCode, ref List<sProductInfo> productInfosList) {
            string sql = "select * from tbl_resultrecord where productCode = @productCodeBar;";
            MySqlParameter paramProductCode = new MySqlParameter();
            MysqlConnector instance = MysqlConnector.GetInstance();
            paramProductCode.ParameterName = "productCodeBar";
            paramProductCode.Value = productCodeBar;
            MySqlParameter[] parameters = new MySqlParameter[] { paramProductCode };
            DataTable dt = instance.GetMySqlRead(sql, parameters);
            if (dt.Rows.Count == 0) { return; }
            for (int i = 0; i < productInfosList.Count(); i++) {
                sProductInfo tmp = productInfosList[i];
                string name = tmp.name;
                if (tmp.name == "mick") {
                    name = name + "1";
                }
                if (dt.Rows[0][name] != null)
                    tmp.value = dt.Rows[0][name].ToString();
                else
                    tmp.value = null;
                productInfosList[i] = tmp;
            }
        }
        /// <summary>
        /// 获取产品对应的装配信息
        /// </summary>
        /// <param name="productCodeInfo"></param>
        /// <returns></returns>
        public static List<sProductInfo> GetProductAssembleFromSQL(string productCodeInfo) {
            string sql = "SELECT * FROM jixing_db.view_partinfo where ProductInfoCode = @productCode;";
            MySqlParameter paramProductCode = new MySqlParameter();
            MysqlConnector instance = MysqlConnector.GetInstance();
            paramProductCode.ParameterName = "productCode";
            paramProductCode.Value = productCodeInfo;
            MySqlParameter[] parameters = new MySqlParameter[] { paramProductCode };
            DataTable dt = instance.GetMySqlRead(sql, parameters);
            List <sProductInfo> productInfosList = new List<sProductInfo>();
            if (dt == null) return null;
            for (int i = 0; i < dt.Rows.Count; i++) {
                string name = Convert.ToString(dt.Rows[i]["Type"]);
                int station = Convert.ToInt32(dt.Rows[i]["Station"]);
                string configCode = Convert.ToString(dt.Rows[i]["ProductInfoCode"]);
                string typeName = Convert.ToString(dt.Rows[i]["TypeName"]);
                int cnt = Convert.ToInt32(dt.Rows[i]["Count"]);
                for (int j = 1; j <= cnt; j++) {
                    sProductInfo info = new sProductInfo();
                    info.station = station;
                    info.ucItem = new UCDetectItem();
                    info.configCode = configCode;
                    if (cnt == 1) {

                        info.typeName = typeName;
                        info.name = name;
                        if (name == "mick") info.name = info.name + 1;
                    }
                    else {
                        info.typeName = typeName + '_' + j;
                        info.name = name + j;
                    }
                    productInfosList.Add(info);
                }
            }
            return productInfosList;
        }
        /// <summary>
        /// 获取产品电流电压范围值
        /// </summary>
        /// <param name="product"></param>
        /// <param name="upper"></param>
        /// <param name="lower"></param>
        /// <param name="upperCurrent"></param>
        public static void GetProductCurrentRange(string product, out float upper, out float lower, out float upperCurrent) {
            string sql = "select RangeUpper,RangeLower,UpperCurrent from view_partinfo where ProductInfoCode = @product";
            MySqlParameter paramProductCode = new MySqlParameter() { ParameterName = "product", Value = product };
            MysqlConnector instance = MysqlConnector.GetInstance();
            MySqlParameter[] parameters = new MySqlParameter[] { paramProductCode };
            DataTable dt = instance.GetMySqlRead(sql, parameters);
            if (dt == null || dt.Rows.Count == 0) {
                upper = 0;
                lower = 0;
                upperCurrent = 0;
                return;
            }
            if (dt.Rows[0]["RangeUpper"].GetType().Name == "DBNull")
                upper = 0;
            else
                upper = Convert.ToSingle(dt.Rows[0]["RangeUpper"]);
            if (dt.Rows[0]["RangeLower"].GetType().Name == "DBNull")
                lower = 0;
            else
                lower = Convert.ToSingle(dt.Rows[0]["RangeLower"]);
            if (dt.Rows[0]["UpperCurrent"].GetType().Name == "DBNull")
                upperCurrent = 0;
            else
                upperCurrent = Convert.ToSingle(dt.Rows[0]["UpperCurrent"]);

            return;
        } 
        /// <summary>
        /// 获取产品信息：产品名
        /// </summary>
        /// <returns></returns>
        public static List<string> GetProductInfosFromSQL() {
            string sql = "select Product from tbl_productconfig;";
            MysqlConnector instance = MysqlConnector.GetInstance();
            DataTable dt = instance.GetMySqlRead(sql, null);
            if (dt == null) return null;
            List<string> productNameList = new List<string>();
            for (int i = 0; i < dt.Rows.Count; i++) {
                string productName =Convert.ToString(dt.Rows[i]["Product"]);
                productNameList.Add(productName);
            }
            return productNameList;
        }
        /// <summary>
        /// 获取零件信息：零件名
        /// </summary>
        /// <returns></returns>
        public static List<string> GetPartTypeFromSQL() {
            string sql = "select TypeName from tbl_parttypes;";
            MysqlConnector instance = MysqlConnector.GetInstance();
            DataTable dt = instance.GetMySqlRead(sql, null);
            if (dt == null) return null;
            List<string> PartTypeList = new List<string>();
            for (int i = 0; i < dt.Rows.Count; i++) {
                string productName = Convert.ToString(dt.Rows[i]["TypeName"]);
                PartTypeList.Add(productName);
            }
            return PartTypeList;
        }
        /// <summary>
        /// 获取零件类型下的所有零件名
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static List<string> GetPartFromSQL(string typeName) {
            string sql = "SELECT * FROM jixing_db.tbl_parts left join tbl_parttypes on tbl_parts.Type = tbl_parttypes.Type where TypeName = @typeName;";
            MySqlParameter paramProductCode = new MySqlParameter();
            MysqlConnector instance = MysqlConnector.GetInstance();
            paramProductCode.ParameterName = "typeName";
            paramProductCode.Value = typeName;
            MySqlParameter[] parameters = new MySqlParameter[] { paramProductCode };
            DataTable dt = instance.GetMySqlRead(sql, parameters);
            List<string> productPartList = new List<string>();
            if (dt == null) return null;
            for (int i = 0; i < dt.Rows.Count; i++) {
                string productName = Convert.ToString(dt.Rows[i]["Name"]);
                productPartList.Add(productName);
            }
            return productPartList;
        }
        /// <summary>
        /// 通过零件名获取零件号
        /// </summary>
        /// <param name="strPartName"></param>
        /// <param name="strPartNum"></param>
        public static void GetPartConfigCode(string strPartName,out string strPartNum) {
            string sql = "SELECT * FROM jixing_db.tbl_parts where Name = @strPartName;";
            MySqlParameter paramProductCode = new MySqlParameter();
            MysqlConnector instance = MysqlConnector.GetInstance();
            paramProductCode.ParameterName = "strPartName";
            paramProductCode.Value = strPartName;
            MySqlParameter[] parameters = new MySqlParameter[] { paramProductCode };
            DataTable dt = instance.GetMySqlRead(sql, parameters);
            if (dt.Rows.Count > 0) {
                strPartNum = (string)dt.Rows[0]["Code"];
            }
            else {
                strPartNum = "";
            }
        }
        /// <summary>
        /// 插入零件信息
        /// </summary>
        /// <param name="strName"></param>
        /// <param name="strPartNum"></param>
        /// <param name="strType"></param>
        public static void InsertPartsInfo(string strName, string strPartNum, string strType) {
            string sql;
            MysqlConnector instance = MysqlConnector.GetInstance();
            MySqlParameter paramName = new MySqlParameter() { ParameterName = "strName", Value = strName };
            MySqlParameter paraPartNum = new MySqlParameter() { ParameterName = "strPartNum", Value = strPartNum };
            MySqlParameter paraType = new MySqlParameter() { ParameterName = "strType", Value = strType };
            MySqlParameter[] parameters;
            parameters = new MySqlParameter[] { paramName, paraPartNum , paraType };
            sql = "SELECT * FROM jixing_db.tbl_parts where Code = @strPartNum and Type = @strType;";
            DataTable dt = instance.GetMySqlRead(sql, parameters);
            if (dt.Rows.Count > 0) {
                sql = "update tbl_parts set Name =@strName where Code = @strPartNum and Type = @strType; ";
            }
            else {
                sql = "insert into tbl_parts(Name, Code, Type) values(@strName, @strPartNum, @strType)";
            }
            int result = instance.ExecuteNonMySQL(sql, parameters);
        }
        /// <summary>
        /// 删除零件信息
        /// </summary>
        /// <param name="strPartNum"></param>
        /// <param name="strType"></param>
        public static void DeletePartsInfo(string strPartNum, string strType) {
            string sql;
            MysqlConnector instance = MysqlConnector.GetInstance();
            MySqlParameter paraPartNum = new MySqlParameter() { ParameterName = "strPartNum", Value = strPartNum };
            MySqlParameter paraType = new MySqlParameter() { ParameterName = "strType", Value = strType };
            MySqlParameter[] parameters;
            parameters = new MySqlParameter[] { paraPartNum, paraType };
            sql = "SELECT * FROM jixing_db.tbl_parts where Code = @strPartNum and Type = @strType;";
            DataTable dt = instance.GetMySqlRead(sql, parameters);
            if (dt.Rows.Count > 0) {
                sql = "delete from tbl_parts  where Code = @strPartNum and Type = @strType; ";
                int result = instance.ExecuteNonMySQL(sql, parameters);
            }
        }
        /// <summary>
        /// 更新零件信息
        /// </summary>
        /// <param name="strName"></param>
        /// <param name="strPartNum"></param>
        /// <param name="strOldePartNum"></param>
        /// <param name="strType"></param>
        public static void UpdatePartsInfo(string strName, string strPartNum, string strOldePartNum, string strType) {
            string sql;
            MysqlConnector instance = MysqlConnector.GetInstance();
            MySqlParameter paramName = new MySqlParameter() { ParameterName = "strName", Value = strName };
            MySqlParameter paraPartNum = new MySqlParameter() { ParameterName = "strPartNum", Value = strPartNum };
            MySqlParameter paraOldPartNum = new MySqlParameter() { ParameterName = "strOldePartNum", Value = strOldePartNum };
            MySqlParameter paraType = new MySqlParameter() { ParameterName = "strType", Value = strType };
            MySqlParameter[] parameters;
            parameters = new MySqlParameter[] { paramName, paraPartNum, paraOldPartNum, paraType };
            sql = "SELECT * FROM jixing_db.tbl_parts where Code = @strPartNum and Type = @strType;";
            DataTable dt = instance.GetMySqlRead(sql, parameters);
            if (dt.Rows.Count > 0) {
                sql = "update tbl_parts set Name =@strName where Code = @strPartNum and Type = @strType; ";
            }
            else {
                sql = "update tbl_parts set Name =@strName ,Code = @strPartNum where Code = @strOldePartNum and Type = @strType; ";
            }
            int  result = instance.ExecuteNonMySQL(sql, parameters);
        }
        /// <summary>
        /// 插入产品信息
        /// </summary>
        /// <param name="strName"></param>
        /// <param name="strProductNum"></param>
        public static void InsertProductInfo(string strName, string strProductNum) {
            string sql;
            MysqlConnector instance = MysqlConnector.GetInstance();
            MySqlParameter paramName = new MySqlParameter() { ParameterName = "strName", Value = strName };
            MySqlParameter paraPartNum = new MySqlParameter() { ParameterName = "strProductNum", Value = strProductNum };
            MySqlParameter[] parameters;
            parameters = new MySqlParameter[] { paramName, paraPartNum };
            sql = "SELECT * FROM jixing_db.tbl_productconfig where ProductCode = @strProductNum ;";
            DataTable dt = instance.GetMySqlRead(sql, parameters);
            if (dt.Rows.Count > 0) {
                sql = "update tbl_productconfig set Product =@strName where ProductCode = @strProductNum; ";
            }
            else {
                sql = "insert into tbl_productconfig(Product, ProductCode) values(@strName, @strProductNum)";
            }
            int result = instance.ExecuteNonMySQL(sql, parameters);
        }
        /// <summary>
        /// 删除产品信息
        /// </summary>
        /// <param name="strProductNum"></param>
        public static void DeleteProductInfo(string strProductNum) {
            string sql;
            MysqlConnector instance = MysqlConnector.GetInstance();
            MySqlParameter paraPartNum = new MySqlParameter() { ParameterName = "strProductNum", Value = strProductNum };
            MySqlParameter[] parameters;
            parameters = new MySqlParameter[] { paraPartNum };
            sql = "SELECT * FROM jixing_db.tbl_productconfig where ProductCode = @strProductNum ;";
            DataTable dt = instance.GetMySqlRead(sql, parameters);
            if (dt.Rows.Count > 0) {
                sql = "delete from tbl_productconfig where ProductCode = @strProductNum; ";
                int result = instance.ExecuteNonMySQL(sql, parameters);
            }
        }
        /// <summary>
        /// 更新产品信息
        /// </summary>
        /// <param name="strName"></param>
        /// <param name="strProductNum"></param>
        /// <param name="strOldProductNum"></param>
        public static void UpdateProductInfo(string strName, string strProductNum, string strOldProductNum) {
            string sql;
            MysqlConnector instance = MysqlConnector.GetInstance();
            MySqlParameter paramName = new MySqlParameter() { ParameterName = "strName", Value = strName };
            MySqlParameter paraProductNum = new MySqlParameter() { ParameterName = "strProductNum", Value = strProductNum };
            MySqlParameter paraProductOldNum = new MySqlParameter() { ParameterName = "strOldProductNum", Value = strOldProductNum };
            MySqlParameter[] parameters;
            parameters = new MySqlParameter[] { paramName, paraProductNum, paraProductOldNum };
            sql = "SELECT * FROM jixing_db.tbl_productconfig where ProductCode = @strProductNum ;";
            DataTable dt = instance.GetMySqlRead(sql, parameters);
            if (dt.Rows.Count > 0) {
                sql = "update tbl_productconfig set Product =@strName where ProductCode = @strProductNum; ";
            }
            else {
                sql = "update tbl_productconfig set Product =@strName,ProductCode =@strProductNum where ProductCode = @strOldProductNum; ";
            }
            int result = instance.ExecuteNonMySQL(sql, parameters);
        }
        /// <summary>
        /// 根据产品名获取产品序列号
        /// </summary>
        /// <param name="productName"></param>
        /// <param name="currentProduct"></param>
        public static void GetProductConfigCodeFromSQL(string productName, out string currentProduct) {
            string sql = "select * from tbl_productconfig where Product = @productName; ";
            MySqlParameter paramProductCode = new MySqlParameter();
            MysqlConnector instance = MysqlConnector.GetInstance();
            paramProductCode.ParameterName = "productName";
            paramProductCode.Value = productName;
            MySqlParameter[] parameters = new MySqlParameter[] { paramProductCode };
            DataTable dt = instance.GetMySqlRead(sql, parameters);
            currentProduct = Convert.ToString(dt.Rows[0]["ProductCode"]);
        }
        /// <summary>
        /// 根据产品名获取装配信息
        /// </summary>
        /// <param name="productName"></param>
        /// <returns></returns>
        public static DataTable GetConfigurationFromSQL(string productName) {
          
            string sql = "SELECT TypeName AS 部件类型, PartName AS 部件名, PartCode AS 部件号, Count AS 数量, Station AS 工位 FROM jixing_db.view_partinfo where Product = @productName; ";
            MySqlParameter paramProductCode = new MySqlParameter();
            MysqlConnector instance = MysqlConnector.GetInstance();
            paramProductCode.ParameterName = "productName";
            paramProductCode.Value = productName;
            MySqlParameter[] parameters = new MySqlParameter[] { paramProductCode };
            DataTable dt = instance.GetMySqlRead(sql, parameters);
            return dt;

        }
        /// <summary>
        /// 是否存在记录
        /// </summary>
        /// <param name="productCodeId"></param>
        /// <returns></returns>
        public static bool HasProductCodeRecord(string productCodeId) {
            string sql = "select * from tbl_resultrecord where productCode = @productCodeId ";
            MySqlParameter paramProductCode = new MySqlParameter();
            MysqlConnector instance = MysqlConnector.GetInstance();
            paramProductCode.ParameterName = "productCodeId";
            paramProductCode.Value = productCodeId;
            MySqlParameter[] parameters = new MySqlParameter[] { paramProductCode };
            DataTable dt = instance.GetMySqlRead(sql, parameters);
            if (dt != null && dt.Rows.Count > 0) {
                return true;
            }
            else {
                return false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="productID"></param>
        /// <param name="partOldName"></param>
        /// <param name="partName"></param>
        /// <param name="count"></param>
        /// <param name="station"></param>
        /// <returns></returns>
        public static int UpdateAssemble(string productID, string partOldName, string partName, int count ,int station) {
            string sql;
            sql = "update tbl_assemble set PartName =@partName ,Count = @count ,Station = @station where ProductID = @productID and PartName=@partOldName ";
            MysqlConnector instance = MysqlConnector.GetInstance();
            MySqlParameter paramPartName = new MySqlParameter() { ParameterName = "partName", Value = partName };
            MySqlParameter paramCount = new MySqlParameter() { ParameterName = "count", Value = count };
            MySqlParameter paramProductName = new MySqlParameter() { ParameterName = "productID", Value = productID };
            MySqlParameter paramPartOldName = new MySqlParameter() { ParameterName = "partOldName", Value = partOldName };
            MySqlParameter paramStation = new MySqlParameter() { ParameterName = "station", Value = station };
            MySqlParameter[] parameters = new MySqlParameter[] { paramPartName , paramCount , paramProductName, paramPartOldName, paramStation};
            int result = instance.ExecuteNonMySQL(sql, parameters);
            return result;

        }
        /// <summary>
        /// 插入装配信息
        /// </summary>
        /// <param name="productName"></param>
        /// <param name="partName"></param>
        /// <param name="count"></param>
        /// <param name="station"></param>
        /// <returns></returns>
        public static int InsertAssemble(string productID, string partName, int count, int station) {
            string sql;
            sql = "insert into tbl_assemble(PartName,Count,ProductID,Station) values(@partName,@count,@productID,@station)";   //SQL语句
            MysqlConnector instance = MysqlConnector.GetInstance();
            MySqlParameter paramPartName = new MySqlParameter() { ParameterName = "partName", Value = partName };
            MySqlParameter paramCount = new MySqlParameter() { ParameterName = "count", Value = count };
            MySqlParameter paramProductName = new MySqlParameter() { ParameterName = "productID", Value = productID };
            MySqlParameter paramStation = new MySqlParameter() { ParameterName = "station", Value = station };
            MySqlParameter[] parameters = new MySqlParameter[] { paramPartName, paramCount, paramProductName, paramStation };
            int result = instance.ExecuteNonMySQL(sql, parameters);
            return result;
        }
        /// <summary>
        /// 删除装配信息
        /// </summary>
        /// <param name="productID"></param>
        /// <param name="partName"></param>
        /// <returns></returns>
        public static int DeleteAssemble(string productID, string partName) {
            string sql;
            sql = "delete from tbl_assemble where ProductID = @productID and PartName = @partName";   //SQL语句
            MysqlConnector instance = MysqlConnector.GetInstance();
            MySqlParameter paramPartName = new MySqlParameter() { ParameterName = "partName", Value = partName };
            MySqlParameter paramProductName = new MySqlParameter() {ParameterName = "productID", Value = productID };
            MySqlParameter[] parameters = new MySqlParameter[] {paramPartName, paramProductName};
            int result = instance.ExecuteNonMySQL(sql, parameters);
            return result;
        }
        /// <summary>
        /// 零件记录是否存在
        /// </summary>
        /// <param name="productCodeId"></param>
        /// <param name="fieldId"></param>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public static bool IsPartCodeRecordExist(string productCodeId, string fieldId ,string newValue) {
            string sql = string.Format("select {0} from tbl_resultrecord where productCode = @productCodeId", fieldId);
            MySqlParameter paramProductCode = new MySqlParameter() { ParameterName = "productCodeId" , Value = productCodeId};
            MysqlConnector instance = MysqlConnector.GetInstance();
            MySqlParameter[] parameters = new MySqlParameter[] { paramProductCode };
            DataTable dt = instance.GetMySqlRead(sql, parameters);
            if (dt != null && dt.Rows.Count > 0 && (string)dt.Rows[0][0].GetType().Name != "DBNull" && (string)dt.Rows[0][0] != newValue) {//如果新增为重复项则认为当前不存在
                return true;
            }
            else {
                return false;
            }
        }
        /// <summary>
        /// 更新产品记录
        /// </summary>
        /// <param name="productCodeId"></param>
        /// <param name="fieldId"></param>
        /// <param name="fieldValue"></param>
        public static bool UpdateProductCodeRecord(string productCodeId, string fieldId, string fieldValue) {
            string sql;
            if (HasProductCodeRecord(productCodeId)) {
                // sql = "update tbl_resultrecord set @fieldId = @fieldValue  where productCode = @productCodeId ";
                sql = string.Format("update tbl_resultrecord set {0} = @fieldValue where productCode = @productCodeId ", fieldId);
                MysqlConnector instance = MysqlConnector.GetInstance();
                MySqlParameter paramPartName = new MySqlParameter() { ParameterName = "fieldId", Value = fieldId };
                MySqlParameter paramCount = new MySqlParameter() { ParameterName = "fieldValue", Value = fieldValue };
                MySqlParameter paramProductName = new MySqlParameter() { ParameterName = "productCodeId", Value = productCodeId };
                MySqlParameter[] parameters = new MySqlParameter[] { paramPartName, paramCount, paramProductName };
                int result = instance.ExecuteNonMySQL(sql, parameters);
                if (result == -1) return false;
               
            }
            return true;
        }
        /// <summary>
        /// 更新产品条码状态
        /// </summary>
        /// <param name="productCodeId"></param>
        /// <param name="currentStatu"></param>
        public static void UpdateProductCodeStatuRecord(string productCodeId,int currentStatu) {
            string sql;
            if (HasProductCodeRecord(productCodeId)) {
                sql = "update tbl_resultrecord set statu =@currentStatu where productCode = @productCodeId ";
                MysqlConnector instance = MysqlConnector.GetInstance();
                MySqlParameter paramProductName = new MySqlParameter() { ParameterName = "productCodeId", Value = productCodeId };
                MySqlParameter paramStation = new MySqlParameter() { ParameterName = "currentStatu", Value = currentStatu };
                MySqlParameter[] parameters = new MySqlParameter[] { paramProductName, paramStation };
                int result = instance.ExecuteNonMySQL(sql, parameters);
            }
        }
        /// <summary>
        /// 插入产品信息
        /// </summary>
        /// <param name="productCodeId"></param>
        /// <param name="productNum"></param>
        /// <param name="currentStatu"></param>
        /// <returns></returns>
        public static int InsertProductCodeRecord(string productCodeId,string productNum, int currentStatu) {
            string sql;
            int result = 0;
            MysqlConnector instance = MysqlConnector.GetInstance();
            MySqlParameter paramDate = new MySqlParameter() { ParameterName = "datetime", Value = DateTime.Now };
            MySqlParameter paraProductCodeId = new MySqlParameter() { ParameterName = "productCodeId", Value = productCodeId };
            MySqlParameter paraProducNum = new MySqlParameter() { ParameterName = "productNum", Value = productNum };
            MySqlParameter paraStatu = new MySqlParameter() { ParameterName = "statu", Value = currentStatu };
            MySqlParameter[] parameters;
            parameters = new MySqlParameter[] { paraProducNum };
            sql = "select Product from tbl_productconfig where ProductCode = @productNum";
            DataTable dt = instance.GetMySqlRead(sql, parameters);
            if (dt == null || dt.Rows.Count == 0) { return 0; }
            string productName = Convert.ToString(dt.Rows[0][0]);
            MySqlParameter paraProductName = new MySqlParameter() { ParameterName = "product", Value = productName };
            parameters = new MySqlParameter[] { paraProductCodeId, paraProductName , paramDate , paraStatu};
            if (!HasProductCodeRecord(productCodeId)) {
                sql = "insert into tbl_resultrecord(datetime,product,productCode,statu) values(@datetime,@product,@productCodeId,@statu)";   //SQL语句
                result = instance.ExecuteNonMySQL(sql, parameters);
            }
            else {
                sql = "update tbl_resultrecord set statu = @statu where productCode = @productCodeId ";    //SQL语句
                result = instance.ExecuteNonMySQL(sql, parameters);
            }
            return result;

        }
        /// <summary>
        /// 检查当前产品零件号是否匹配
        /// </summary>
        /// <param name="currentProductNum"></param>
        /// <param name="inputCode"></param>
        /// <param name="station"></param>
        /// <returns></returns>
        public static string CheckPartCodeType(string currentProductNum,string inputCode,int station) {
            string sql = "SELECT Type FROM jixing_db.view_partinfo where PartCode = @PartCode && ProductInfoCode = @ProductInfoCode && Station = @station";
            MySqlParameter paramProductCode = new MySqlParameter() { ParameterName = "ProductInfoCode", Value = currentProductNum };
            MySqlParameter paramPartCode = new MySqlParameter() { ParameterName = "PartCode", Value = inputCode };
            MySqlParameter paramStation = new MySqlParameter() { ParameterName = "station", Value = station };
            MysqlConnector instance = MysqlConnector.GetInstance();
            DataTable dt = instance.GetMySqlRead(sql,new MySqlParameter[]{ paramProductCode, paramPartCode, paramStation});
            if (dt.Rows.Count > 0) return dt.Rows[0][0].ToString();
            return "Null";
        }
        /// <summary>
        /// 设置电流电压范围
        /// </summary>
        /// <param name="productCode"></param>
        /// <param name="upper"></param>
        /// <param name="lower"></param>
        /// <param name="upperCurrent"></param>
        public static void SetCurrentValue(string productCode, float upper, float lower,float upperCurrent) {
            string sql;
            MysqlConnector instance = MysqlConnector.GetInstance();
            MySqlParameter paraProductCodeId = new MySqlParameter() { ParameterName = "productCode", Value = productCode };
            MySqlParameter paraLower = new MySqlParameter() { ParameterName = "lower", Value = lower };
            MySqlParameter paraUpper = new MySqlParameter() { ParameterName = "upper", Value = upper };
            MySqlParameter paraUpperCurrent = new MySqlParameter() { ParameterName = "upperCurrent", Value = upperCurrent };
            MySqlParameter[] parameters;
            parameters = new MySqlParameter[] { paraProductCodeId, paraLower, paraUpper, paraUpperCurrent };
            sql = "select * from tbl_currentrange where Product = @productCode";
            DataTable dt = instance.GetMySqlRead(sql, parameters);
            if (dt.Rows.Count > 0) {
                sql = "update tbl_currentrange set RangeUpper = @upper , RangeLower =@lower, UpperCurrent = @upperCurrent where Product = @productCode ";
            }
            else {
                sql = "insert into tbl_currentrange(RangeUpper,RangeLower,UpperCurrent,Product) values(@upper,@lower,@upperCurrent,@productCode)";   //SQL语句
            }
            instance.ExecuteNonMySQL(sql, parameters);
        }
        /// <summary>
        /// 获取零件配置信息
        /// </summary>
        /// <param name="productCode"></param>
        /// <param name="partCode"></param>
        /// <returns></returns>
        public static DataTable  GetPartConfigType(string productCode,string partCode) {
            string sql = " SELECT Type,Count FROM jixing_db.view_partinfo where ProductInfoCode =@productCode and PartCode = @partCode";
            MySqlParameter paramProductCode = new MySqlParameter() { ParameterName = "productCode", Value = productCode };
            MySqlParameter paramPartCode = new MySqlParameter() { ParameterName = "partCode", Value = partCode };
            MysqlConnector instance = MysqlConnector.GetInstance();
            MySqlParameter[] parameters = new MySqlParameter[] { paramProductCode, paramPartCode };
            return instance.GetMySqlRead(sql, parameters);

        }
        /// <summary>
        /// 获取当前工位正在运行产品
        /// </summary>
        /// <param name="statuStart"></param>
        /// <param name="statuEnd"></param>
        /// <returns></returns>
        public static DataTable GetCurrentRunningProduct(int statuStart, int statuEnd) {
            string sql = " SELECT * FROM jixing_db.tbl_resultrecord where statu >= @statuStart and statu < @statuEnd";
            MySqlParameter paramStatuStart = new MySqlParameter() { ParameterName = "statuStart", Value = statuStart };
            MySqlParameter paramStatuEnd = new MySqlParameter() { ParameterName = "statuEnd", Value = statuEnd };
            MysqlConnector instance = MysqlConnector.GetInstance();
            MySqlParameter[] parameters = new MySqlParameter[] { paramStatuStart, paramStatuEnd };
            return instance.GetMySqlRead(sql, parameters);
        }
        /// <summary>
        /// 通过零件名获取零件类型
        /// </summary>
        /// <param name="partTypeName"></param>
        /// <param name="partType"></param>
        public static void GetPartTypeForPartName(string partTypeName ,out string partType) {
            string sql = " SELECT Type FROM jixing_db.tbl_parttypes where TypeName = @partTypeName ;";
            MySqlParameter paramPartTypeName = new MySqlParameter() { ParameterName = "partTypeName", Value = partTypeName };
            MysqlConnector instance = MysqlConnector.GetInstance();
            MySqlParameter[] parameters = new MySqlParameter[] { paramPartTypeName };
            DataTable dt = instance.GetMySqlRead(sql, parameters);
            if (dt.Rows.Count > 0) {
                partType = (string)dt.Rows[0][0];
            }
            else {
                partType = "";
            }
        }
        /// <summary>
        /// 获取产品条码状态
        /// </summary>
        /// <param name="strProductCode"></param>
        /// <returns></returns>
        public static int GetProductCodeStatu(string strProductCode) {
            string sql = " SELECT statu FROM jixing_db.tbl_resultrecord where productCode = @productCode ;";
            MySqlParameter paramPartTypeName = new MySqlParameter() { ParameterName = "productCode", Value = strProductCode };
            MysqlConnector instance = MysqlConnector.GetInstance();
            MySqlParameter[] parameters = new MySqlParameter[] { paramPartTypeName };
            DataTable dt = instance.GetMySqlRead(sql, parameters);
            if (dt.Rows.Count > 0) {
                return (int)dt.Rows[0][0];
            }
            return (int)eStation1_WorkProcess.Waiting;
        }
        /// <summary>
        /// 删除产品条码记录
        /// </summary>
        /// <param name="strProductCodeId"></param>
        public static void DeleteProductCodeRecode(string strProductCodeId) {
            string sql;
            sql = string.Format("delete from tbl_resultrecord where productCode = @productCodeId ");
            MysqlConnector instance = MysqlConnector.GetInstance();
            MySqlParameter paramProductName = new MySqlParameter() { ParameterName = "productCodeId", Value = strProductCodeId };
            MySqlParameter[] parameters = new MySqlParameter[] { paramProductName };
            int result = instance.ExecuteNonMySQL(sql, parameters);

        }
    }
}
