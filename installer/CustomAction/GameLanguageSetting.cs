using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace TuringCompleteJpCA
{
    /// <summary>
    /// ゲームのセーブデータ progress.dat (SQLite) にある言語設定 (progress.key = 'setting_language') の読み書き。
    /// Windows 10 以降に標準で入っている winsqlite3.dll を直接呼ぶ。
    /// System.Data.SQLite はネイティブ interop DLL が DTF の CA パッケージに入らず、ロードに失敗するため使わない (#5)。
    /// </summary>
    static class GameLanguageSetting
    {
        const string Key = "setting_language";

        public static string DbPath
        {
            get
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                return Path.Combine(appData, "godot", "app_userdata", "Turing Complete", "progress.dat");
            }
        }

        /// <summary>
        /// 言語設定を書き込む。行があれば UPDATE、なければ INSERT する（テーブルに主キーがないため UPSERT は使えない）。
        /// 戻り値は変更した行数。progress.dat がなければ例外 FileNotFoundException。
        /// </summary>
        public static int Write(string language)
        {
            return Write(DbPath, language);
        }

        internal static int Write(string path, string language)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("progress.dat not found", path);

            IntPtr db = Open(path);
            try
            {
                Exec(db, "BEGIN IMMEDIATE", null);
                try
                {
                    int rows = Exec(db, "UPDATE progress SET value = ?1 WHERE key = '" + Key + "'", language);
                    if (rows == 0)
                        rows = Exec(db, "INSERT INTO progress (key, value) VALUES ('" + Key + "', ?1)", language);
                    Exec(db, "COMMIT", null);
                    return rows;
                }
                catch
                {
                    Native.sqlite3_exec(db, Utf8("ROLLBACK"), IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
                    throw;
                }
            }
            finally
            {
                Native.sqlite3_close_v2(db);
            }
        }

        static IntPtr Open(string path)
        {
            IntPtr db;
            int rc = Native.sqlite3_open_v2(Utf8(path), out db, Native.SQLITE_OPEN_READWRITE, IntPtr.Zero);
            if (rc != Native.SQLITE_OK)
            {
                string msg = db != IntPtr.Zero ? ErrMsg(db) : "rc=" + rc;
                if (db != IntPtr.Zero) Native.sqlite3_close_v2(db);
                throw new InvalidOperationException("sqlite3_open_v2 failed: " + msg);
            }
            // ゲームが起動中でロックされている場合に備えて少し待つ
            Native.sqlite3_busy_timeout(db, 3000);
            return db;
        }

        /// <summary>1 文を実行し、変更行数を返す。parameter が null でなければ ?1 にバインドする。</summary>
        static int Exec(IntPtr db, string sql, string parameter)
        {
            IntPtr stmt;
            int rc = Native.sqlite3_prepare_v2(db, Utf8(sql), -1, out stmt, IntPtr.Zero);
            if (rc != Native.SQLITE_OK)
                throw new InvalidOperationException("prepare failed (" + sql + "): " + ErrMsg(db));
            try
            {
                if (parameter != null)
                {
                    byte[] value = Encoding.UTF8.GetBytes(parameter);
                    rc = Native.sqlite3_bind_text(stmt, 1, value, value.Length, Native.SQLITE_TRANSIENT);
                    if (rc != Native.SQLITE_OK)
                        throw new InvalidOperationException("bind failed: " + ErrMsg(db));
                }
                rc = Native.sqlite3_step(stmt);
                if (rc != Native.SQLITE_DONE)
                    throw new InvalidOperationException("step failed (" + sql + "): " + ErrMsg(db));
                return Native.sqlite3_changes(db);
            }
            finally
            {
                Native.sqlite3_finalize(stmt);
            }
        }

        static byte[] Utf8(string s)
        {
            return Encoding.UTF8.GetBytes(s + "\0");
        }

        static string ErrMsg(IntPtr db)
        {
            IntPtr p = Native.sqlite3_errmsg(db);
            if (p == IntPtr.Zero) return "(no message)";
            int len = 0;
            while (Marshal.ReadByte(p, len) != 0) len++;
            byte[] buf = new byte[len];
            Marshal.Copy(p, buf, 0, len);
            return Encoding.UTF8.GetString(buf);
        }

        /// <summary>winsqlite3.dll は x86 で __stdcall（WINAPI）を使う。x64 では呼び出し規約の指定は無視される。</summary>
        static class Native
        {
            const string Dll = "winsqlite3.dll";
            public const int SQLITE_OK = 0;
            public const int SQLITE_DONE = 101;
            public const int SQLITE_OPEN_READWRITE = 0x00000002;
            public static readonly IntPtr SQLITE_TRANSIENT = new IntPtr(-1);

            [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
            public static extern int sqlite3_open_v2(byte[] filename, out IntPtr db, int flags, IntPtr vfs);

            [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
            public static extern int sqlite3_close_v2(IntPtr db);

            [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
            public static extern int sqlite3_busy_timeout(IntPtr db, int ms);

            [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
            public static extern int sqlite3_prepare_v2(IntPtr db, byte[] sql, int nByte, out IntPtr stmt, IntPtr tail);

            [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
            public static extern int sqlite3_bind_text(IntPtr stmt, int index, byte[] value, int nByte, IntPtr destructor);

            [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
            public static extern int sqlite3_step(IntPtr stmt);

            [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
            public static extern int sqlite3_finalize(IntPtr stmt);

            [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
            public static extern int sqlite3_changes(IntPtr db);

            [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
            public static extern int sqlite3_exec(IntPtr db, byte[] sql, IntPtr callback, IntPtr arg, IntPtr errmsg);

            [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
            public static extern IntPtr sqlite3_errmsg(IntPtr db);
        }
    }
}
