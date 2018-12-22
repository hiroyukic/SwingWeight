Imports System.Windows.Forms.DataVisualization.Charting

Public Class Form1
    Private dataTable As DataTable
    Dim COM_BALANCE_POINT As String = "Balance Point [cm]"
    Dim COM_WEIGHT0 As Double = 0
    Dim COM_FREQ0 As Double
    Dim COM_BP0 As Double
    Dim COM_WEIGHT1 As Double = 0
    Dim COM_FREQ1 As Double
    Dim COM_BP1 As Double

    ''' ------------------------------------------------------------------------
    ''' <summary>
    '''     「計算」ボタン押下 </summary>
    ''' ------------------------------------------------------------------------
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        '重量[g]
        Dim weight As Double
        '重心
        Dim posg As Double
        Try
            weight = TextBox1.Text
            posg = TextBox2.Text
        Catch ex As Exception
            Return
        End Try

        Try
            'ＳＷライン削除
            Chart1.ChartAreas(0).AxisY.StripLines.RemoveAt(0)
        Catch ex As Exception
        End Try

        '振動周波数
        Dim lengthType As String = ComboBox1.SelectedItem
        Debug.WriteLine(lengthType)
        Dim VibrationFreq As Double

        If (lengthType = "Normal") Then
            VibrationFreq = 1.38
        Else
            VibrationFreq = 1.4
        End If

        COM_WEIGHT1 = weight
        COM_FREQ1 = VibrationFreq
        COM_BP1 = posg

        '=============================================================
        Me.Chart1.Series.Clear() ' Series のクリア
        Me.CreateData(COM_WEIGHT0, COM_BP0, COM_FREQ0, COM_WEIGHT1, COM_BP1, COM_FREQ1)
        Me.CreateChart()
        Me.Chart1.DataSource = Me.dataTable

        '横線追加
        Dim swingWeight As Double
        swingWeight = getDataSet4SwingWeight(weight / 1000, posg, VibrationFreq)
        AddHorizontalLine(Chart1.ChartAreas(0), swingWeight)

        Dim message As String
        message = "Length Type=" & lengthType
        ListBox1.Items.Add(message)

        message = "Weight=" & COM_WEIGHT1
        ListBox1.Items.Add(message)

        message = "Center=" & COM_BP1
        ListBox1.Items.Add(message)

        message = "SwingWeight=" & CStr(ToHalfAdjust(swingWeight, 1)) & " [kg.c㎡]"
        ListBox1.Items.Add(message)

        message = "-"
        ListBox1.Items.Add(message)
        message = " "
        ListBox1.Items.Add(message)

        If (swingWeight < 300 Or swingWeight > 440) Then
            message = "計算結果が描画範囲を超えています。"
            ListBox1.Items.Add(message)
            message = "適正なパラメータを入力して下さい。"
            ListBox1.Items.Add(message)
            message = "-"
            ListBox1.Items.Add(message)
        End If

        COM_WEIGHT0 = COM_WEIGHT1
        COM_FREQ0 = COM_FREQ1
        COM_BP0 = COM_BP1
    End Sub


    ''' ------------------------------------------------------------------------
    ''' <summary>
    '''     前回のパラメータでの計算値と今回のパラメータでの計算値を求める。</summary>
    ''' <param name="weight0">
    '''     前回の質量[g]</param>
    ''' <param name="balance0">
    '''     前回のバランスポイント[cm]</param>
    ''' <param name="freq0">
    '''     前回の振動周期[sec]</param>
    ''' <param name="weight1">
    '''     今回の質量[g]</param>
    ''' <param name="balance1">
    '''     今回のバランスポイント[cm]</param>
    ''' <param name="freq1">
    '''     今回の振動周期[sec]</param>
    '''     
    ''' ------------------------------------------------------------------------
    Private Sub CreateData(weight0 As Double, balance0 As Double, freq0 As Double, weight1 As Double, balance1 As Double, freq1 As Double)
        Me.dataTable = New DataTable("データ")
        Me.dataTable.Columns.Add("BP", GetType(Double))
        Me.dataTable.Columns.Add("SW0", GetType(Double))
        Me.dataTable.Columns.Add("SW1", GetType(Double))

        ' データの初期値を作成する
        Dim swingWeight0 As Double
        Dim swingWeight1 As Double

        '--------------------------------------------------------------
        ' データ作成
        '--------------------------------------------------------------
        For bp As Integer = 285 To 350 Step 5
            Debug.Print(bp)
            ' データを設定する
            'Me.dataTable.Rows.Add(d, high, low, open, close, volume)
            'Me.dataTable.Rows.Add(d, high, low, open, 0, volume)

            ' 次に設定するデータを用意する
            'd = d.AddDays(1)

            If (weight0 = 0) Then
                swingWeight0 = 300
            Else
                swingWeight0 = getDataSet4SwingWeight(weight0 / 1000, bp / 10, freq0)
            End If

            swingWeight1 = getDataSet4SwingWeight(weight1 / 1000, bp / 10, freq1)
            Me.dataTable.Rows.Add(bp / 10, swingWeight0, swingWeight1)

            Debug.Print(bp & " " & swingWeight0 & "  " & swingWeight1)
        Next
    End Sub


    ''' ------------------------------------------------------------------------
    ''' <summary>
    '''  チャートを作る</summary>
    ''' ------------------------------------------------------------------------
    Private Sub CreateChart()

        Me.Chart1.ChartAreas(0).AxisY.Maximum = 450
        Me.Chart1.ChartAreas(0).AxisY.Minimum = 300
        Me.Chart1.ChartAreas(0).AxisY.Interval = 20
        Me.Chart1.ChartAreas(0).AxisY2.Maximum = 450
        Me.Chart1.ChartAreas(0).AxisY2.Minimum = 300
        Me.Chart1.ChartAreas(0).AxisY2.Interval = 20

        ' グラフ１（今回）--------------------------------------------------------------------------
        Dim swChart1
        Try
            swChart1 = Me.Chart1.Series.Add(COM_WEIGHT1.ToString)
        Catch ex As Exception
            swChart1 = Me.Chart1.Series.Add("Latest")
        End Try

        swChart1.XValueMember = "BP"
        swChart1.YValueMembers = "SW1"

        swChart1.ChartType = DataVisualization.Charting.SeriesChartType.Line ' (１) 縦棒
        swChart1.YAxisType = DataVisualization.Charting.AxisType.Secondary ' (２) 参照する軸を分ける
        swChart1.XValueType = DataVisualization.Charting.ChartValueType.Int32
        swChart1.YValueType = DataVisualization.Charting.ChartValueType.Double
        swChart1.ChartArea = Me.Chart1.ChartAreas(0).Name ' 同じ描画領域に描く
        swChart1.Color = Color.Red
        swChart1.Enabled = True

        ' グラフ０ (前回）--------------------------------------------------------------------------
        Dim swChart0
        Try
            swChart0 = Me.Chart1.Series.Add(COM_WEIGHT0.ToString)
        Catch ex As Exception
            swChart0 = Me.Chart1.Series.Add("Prev.")
        End Try

        swChart0.XValueMember = "BP"
        swChart0.YValueMembers = "SW0"
        swChart0.ChartType = DataVisualization.Charting.SeriesChartType.Line
        swChart0.YAxisType = DataVisualization.Charting.AxisType.Primary ' (２) 参照する軸を分ける
        swChart0.XValueType = DataVisualization.Charting.ChartValueType.Int32
        swChart0.YValueType = DataVisualization.Charting.ChartValueType.Double
        swChart0.ChartArea = Me.Chart1.ChartAreas(0).Name ' 同じ描画領域に描く
        swChart0.Enabled = True

    End Sub


    ''' ------------------------------------------------------------------------
    ''' <summary>
    '''  フォーム初期化
    '''  </summary>
    ''' ------------------------------------------------------------------------
    Private Sub Form1_Load(ByVal sender As Object, ByVal e As EventArgs) Handles MyBase.Load
        Me.CreateData(0, COM_BP0, COM_FREQ0, COM_WEIGHT1, COM_BP1, COM_FREQ1)
        Me.CreateChart()
        Me.Chart1.DataSource = Me.dataTable

        'コンボボックス　Normal/Long
        With ComboBox1
            .Items.Add("Normal")
            .Items.Add("Long")
            .Font = New Font("ＭＳ Ｐゴシック", 8)
            .Text = "Normal" 'テキストボックスに最初に表示して置く項目を設定
        End With

        'タブ名称
        Me.TabPage1.Text = "S.W. Graph"
        Me.TabPage2.Text = "Readme"

        Me.Chart1.Series.Clear() ' (４) Series のクリアは最初だけ
        'Me.CreateData()
        Me.CreateChart()
        Me.Chart1.DataSource = Me.dataTable
        'Me.Chart1.ChartAreas(0).AxisY2.Maximum = 400 ' (２) 有効にすると、棒グラフが下がる

        With Chart1.ChartAreas(0).AxisY
            '目盛りの設定と目盛線の設定
            .Maximum = 450    '点数の最大値
            .Minimum = 300      '点数の最小値
            .Interval = 20    '点数のメモリ間隔(２０点毎)
            '"#,###" と設定すると基点の 0 が表示されない。
            .LabelStyle.Format = "#,##0"  '桁区切りで表示の場合

            '補助線を表示
            '.MinorGrid.Enabled = True  'True に設定しないと表示しない

            'ラベルに対して自動的に適用できるスタイルの設定
            .LabelAutoFitMaxFontSize = 10

            'Y軸のラベルとプロットエリアとの境界の縦線
            .LineWidth = 1
            .LineColor = Color.Blue
            .LineDashStyle = DataVisualization.Charting.ChartDashStyle.Solid

            'Y軸のタイトル関係の設定
            .Title = "Swing Weight [kg c㎡]"
            .TitleAlignment = StringAlignment.Center        '中央に表示
            '.TextOrientation = TextOrientation.Stacked      '縦書きで表示
            '.TitleFont = New Font("ＭＳ Ｐ明朝", 10, FontStyle.Regular)
        End With

        With Chart1.ChartAreas(0).AxisX
            .Interval = 1   '点数のメモリ間隔(２０点毎)
            '"#,###" と設定すると基点の 0 が表示されない。
            .LabelStyle.Format = "#,##0"  '桁区切りで表示の場合

            '補助線
            .MinorGrid.Enabled = True  'True に設定しないと表示しない
            .MinorGrid.Interval = 2

            'ラベルスタイルの設定
            .LabelAutoFitMaxFontSize = 10

            'Y軸のラベルとプロットエリアとの境界の縦線
            .LineWidth = 1.2
            .LineColor = Color.Blue
            .LineDashStyle = DataVisualization.Charting.ChartDashStyle.Solid

            'Y軸のタイトル関係の設定
            .Title = COM_BALANCE_POINT

            .TitleAlignment = StringAlignment.Center        '中央に表示
            '.TextOrientation = TextOrientation.Stacked      '縦書きで表示
            '.TitleFont = New Font("ＭＳ Ｐ明朝", 10, FontStyle.Regular)
        End With

    End Sub


    ''' ------------------------------------------------------------------------
    ''' <summary>
    '''     スイングウエイトの算出関数</summary>
    ''' <param name="weight">
    '''     質量[g]</param>
    ''' <param name="glength">
    '''     バランスポイント[cm]</param>
    ''' <param name="cycleTime">
    '''     振動周期[sec]</param>  
    ''' ------------------------------------------------------------------------
    Private Function getDataSet4SwingWeight(weight As Double, glength As Double, cycleTime As Double) As Double
        Dim ds As New DataSet
        Dim dt As New DataTable

        Dim Ig1 As Double
        Dim Ig2 As Double

        Ig1 = (weight * 980 * glength * (cycleTime ^ 2)) / (4 * (3.1415 ^ 2))
        Ig2 = weight * (glength ^ 2)

        '慣性モーメント　Ig = Ig1 - Ig2
        Dim Ig As Double
        Ig = Ig1 - Ig2

        '慣性モーメント（軸まわり）　Io = Ig + Lg^2 * M
        Dim Io As Double
        Io = Ig + ((glength - 7.5) ^ 2) * weight
        System.Diagnostics.Debug.WriteLine(Io)

        Return (Io)
    End Function


    ''' ------------------------------------------------------------------------
    ''' <summary>
    '''     指定した精度の数値に四捨五入</summary>
    ''' <param name="dValue">
    '''     丸め対象の倍精度浮動小数点数。</param>
    ''' <param name="iDigits">
    '''     戻り値の有効桁数の精度。</param>
    ''' <returns>
    '''     iDigits に等しい精度の数値に四捨五入された数値。</returns>
    ''' ------------------------------------------------------------------------
    Public Shared Function ToHalfAdjust(ByVal dValue As Double, ByVal iDigits As Integer) As Double
        Dim dCoef As Double = System.Math.Pow(10, iDigits)

        If dValue > 0 Then
            Return System.Math.Floor((dValue * dCoef) + 0.5) / dCoef
        Else
            Return System.Math.Ceiling((dValue * dCoef) - 0.5) / dCoef
        End If
    End Function



    ''' ------------------------------------------------------------------------
    ''' <summary>
    ''' 指定したチャートエリアの指定したy座標に赤い横線を追加するメソッド</summary>
    ''' ------------------------------------------------------------------------
    Private Sub AddHorizontalLine(area As ChartArea, yValue As Double)
        Dim line As StripLine = New StripLine()

        line.BorderColor = Color.Red
        line.IntervalOffset = yValue
        line.Interval = 0
        line.BorderWidth = 1

        line.BorderDashStyle = DataVisualization.Charting.ChartDashStyle.Dash

        area.AxisY.StripLines.Add(line)

    End Sub

End Class
