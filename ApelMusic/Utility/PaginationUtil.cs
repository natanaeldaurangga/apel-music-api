using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace ApelMusic.Utility
{
    public static class PaginationUtil // static class tidak bisa di instansiate (dibikinin objek)
    {

        // Untuk sementara tidak digunakan
        public static int CalculatePage(int totalRowsInTable, int pageSize)
        {
            decimal perPage = (decimal)totalRowsInTable / pageSize;
            perPage = Math.Ceiling(perPage);
            return (int)perPage;
        }

        #region METHOD UNTUK INSERT
        // public async Task<int> InsertPaymentTaskAsync(SqlConnection conn, SqlTransaction transaction, )
        #endregion

    }
}