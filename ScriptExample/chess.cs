var chess_current_board = new string[12][] {
	new string[]{"１","２","３","４","５","６","７","８","９"},
	new string[]{"車","馬","象","士","將","士","象","馬","車"},
	new string[]{"　","　","　","　","　","　","　","　","　"},
	new string[]{"　","炮","　","　","　","　","　","炮","　"},
	new string[]{"卒","　","卒","　","卒","　","卒","　","卒"},
	new string[]{"　","　","　","　","　","　","　","　","　"},
	new string[]{"　","　","　","　","　","　","　","　","　"},
	new string[]{"兵","　","兵","　","兵","　","兵","　","兵"},
	new string[]{"　","砲","　","　","　","　","　","砲","　"},
	new string[]{"　","　","　","　","　","　","　","　","　"},
	new string[]{"俥","傌","相","仕","帥","仕","相","傌","俥"},
	new string[]{"九","八","七","六","五","四","三","二","一"}
};
var chess_current_step = 1;

var chess_reset = () => {
	chess_current_board = new string[12][] {
		new string[]{"１","２","３","４","５","６","７","８","９"},
		new string[]{"車","馬","象","士","將","士","象","馬","車"},
		new string[]{"　","　","　","　","　","　","　","　","　"},
		new string[]{"　","炮","　","　","　","　","　","炮","　"},
		new string[]{"卒","　","卒","　","卒","　","卒","　","卒"},
		new string[]{"　","　","　","　","　","　","　","　","　"},
		new string[]{"　","　","　","　","　","　","　","　","　"},
		new string[]{"兵","　","兵","　","兵","　","兵","　","兵"},
		new string[]{"　","砲","　","　","　","　","　","砲","　"},
		new string[]{"　","　","　","　","　","　","　","　","　"},
		new string[]{"俥","傌","相","仕","帥","仕","相","傌","俥"},
		new string[]{"九","八","七","六","五","四","三","二","一"}
	};
	chess_current_step = 1;
};
var chess_current_board_string = string() => {
	var str = "";
	var ccb = chess_current_board;
	for (int i = 0; i < ccb.Length; i++) {
		for (int j = 0; j < ccb[i].Length; j++)
			str += ccb[i][j];
		str += "\n";
	}
	return str;
};

var chess_play = string(string input) => {
	var ccb = chess_current_board;
	var step = chess_current_step;
	var str = "";
	var red = "俥傌相仕帥仕相傌俥砲兵";
	var black = "車馬象士將士象馬車炮卒";
	var a = input[0].ToString();
	var b = input[1].ToString();
	var c = input[2].ToString();
	var d = int.Parse(input[3].ToString());
	int m = -1, n = -1, dm = 0, dn = 0;
	var koma = "";
	var e = new Exception("违反棋规，不能下这一步");
	try {
		if("前后".Contains(a)){
			for(var i=1;i<=10;i++) {
				if(Array.IndexOf(ccb[i],b) > -1) {
					m=Array.IndexOf(ccb[i],b);
					n=i;
					if(a=="后"&&step%2==0)
						break;
					if(a=="前"&&step%2==1)
						break;
				}
			}
			koma=b;
		} else{
			for(var i=1;1<=10;i++) {
				var j=step%2 > 0 ? 9-int.Parse(b) : int.Parse(b)-1;
				if(ccb[i][j]==a){
					m=j;
					n=i;
					break;
				}
			}
			koma=a;
		}
		if(m<0||n<0)throw e;
		if("將帥兵卒".Contains(koma)&&d!= 1&&c!="平")throw e;//这些最多走一步
		if(c=="平"){
			if("傌馬象士相仕".Contains(koma))throw e;//这些不能平
			if("兵卒".Contains(koma)) {//兵过河前不能平
				if(step%2>0&&n>6)throw e;
				if(step%2==0&&n<=6)throw e;
			}
			dm=step%2>0?9-d:d-1;
			dn=n;
			if("將帥兵卒".Contains(koma)&&Math.Abs(dm-m)!=1)throw e;
		} else if(c=="进"){
			if ("象相".Contains(koma)) {
				dm = step%2 > 0 ? 9-d : d-1;
				dn = step%2 > 0 ? n-2 : n+2;
				var minus = Math.Abs(dm-m);
				if (minus != 2) throw e;
			}
			if ("士仕".Contains(koma)) {
				dm = step%2 > 0 ? 9-d : d-1;
				dn = step%2 > 0 ? n-1 : n+1;
				var minus = Math.Abs(dm-m);
				if (minus != 1) throw e;
			}
			if ("傌馬".Contains(koma)) {
				dm = step%2 > 0 ? 9-d : d-1;
				var minus = Math.Abs(dm-m);
				if (minus == 0 || minus > 2) throw e;
				if (minus == 1) {
					dn = step%2 > 0 ? n-2 : n+2;
				} else {
					dn = step%2 > 0 ? n-1 : n+1;
				}
			}
			if ("車俥炮砲將帥兵卒".Contains(koma)) {
				dm = m;
				dn = step%2 > 0 ? n-d : n+d;
			}
		} else if (c == "退") {
			if ("兵卒".Contains(koma)) throw e;//兵不能退
			if ("象相".Contains(koma)) {
				dm = step%2 > 0 ? 9-d : d-1;
				dn = step%2 == 0 ? n-2 : n+2;
				var minus = Math.Abs(dm-m);
				if (minus != 2) throw e;
			}
			if ("士仕".Contains(koma)) {
				dm = step%2 > 0 ? 9-d : d-1;
				dn = step%2 == 0 ? n-1 : n+1;
				var minus = Math.Abs(dm-m);
				if (minus != 1) throw e;
			}
			if ("傌馬".Contains(koma)) {
				dm = step%2 > 0 ? 9-d : d-1;
				var minus = Math.Abs(dm-m);
				if (minus == 0 || minus > 2) throw e;
				if (minus == 1) {
					dn = step%2 == 0 ? n-2 : n+2;
				} else {
					dn = step%2 == 0 ? n-1 : n+1;
				}
			}
			if ("車俥炮砲將帥兵卒".Contains(koma)) {
				dm = m;
				dn = step%2 == 0 ? n-d : n+d;
			}
		}else throw e;
		if(dm<0||dm>=9)throw e;//不能走到棋盘外
		if(dn<1||dn>=11)throw e;
		if(ccb[dn][dm]!="　") {//不能吃自己子
			if(step%2>0&&red.Contains(ccb[dn][dm]))throw e;
			if(step%2==0&&black.Contains(ccb[dn][dm]))throw e;
		}
		if("將帥士仕".Contains(koma)){//将士不出九宫
			if (dm < 3 || dm > 5 || (dn > 4 && dn < 8))throw e;
		}
		if("象相".Contains(koma)) {//象不能过河
			if(step%2>0&&dn<6)throw e;
			if(step%2==0&&dn>=6)throw e;
			int midm=(dm+m)/2,midn=(dn+n)/2;//象眼
			if(ccb[midn][midm]!="　")throw e;
		}
		if("傌馬".Contains(koma)) {//马脚
			int midm=0,midn=0;
			if(Math.Abs(dm-m)==1){
				midn=(dn+n)/2;
				midm=m;
			}else{
				midm=(dm+m)/2;
				midn=n;
			}
			if(ccb[midn][midm]!="　")throw e;
		}
		if("車俥".Contains(koma)){
			if(dn==n) {//平
				var i=Math.Min(m,dm)+1;
				while (i<Math.Max(m,dm)){
					if(ccb[dn][i]!="　")throw e;
					i++;
				}
			}else{//进退
				var i=Math.Min(n,dn)+1;
				while(i<Math.Max(n,dn)){
					if(ccb[i][dm]!="　")throw e;
					i++;
				}
			}
		}
		if("炮砲".Contains(koma)){
			if(dn == n){//平
				int j=0,i=Math.Min(m,dm)+1;
				while (i<Math.Max(m,dm)){
					if(ccb[dn][i]!="　") j++;
					i++;
				}
				if(j>=2||(j>0&&ccb[dn][dm]=="　")||(j==0&&ccb[dn][dm]!="　"))throw e;
			}else{//进退
				int j=0,i=Math.Min(n,dn)+1;
				while(i<Math.Max(n,dn)){
					if(ccb[i][dm]!="　") j++;
					i++;
				}
				if(j>=2||(j>0&&ccb[dn][dm]=="　")||(j==0&&ccb[dn][dm]!="　"))throw e;
			}
		}
		if(ccb[dn][dm]=="帥")
			return "游戏结束，黑(將)方胜";
		if(ccb[dn][dm]=="將")
			return "游戏结束，红(帥)方胜";
		ccb[n][m]="　";
		ccb[dn][dm]=koma;
	} catch (Exception err) {
		return err.Message;
	}
	var cur = step % 2 > 0 ? "红" : "黑";
	var next = step % 2 == 0 ? "红" : "黑";
	chess_current_step++;
	str += cur + input + "，轮到" + next + "方。查看棋局输入: /chess help";
	return str;
};

var chess = string(string input) => {
	var help = "象棋相关指令>>\n开局：/chess reset\n下棋：/chess 炮2平5\n查看：/chess help";
	var ccb = chess_current_board;
	var step = chess_current_step;
	var str = "";
	if (input == "help") {
		var cur = step % 2 > 0 ? "红" : "黑";
		str += $"当前棋局(轮到{cur}方)>>\n" + chess_current_board_string();
		str += "\n" + help;
	} else if (input == "reset") {
		chess_reset();
		str += "新的棋局开始了！红先>>\n" + chess_current_board_string();
		str += "\n" + help;
	} else {
		input = input.Trim();
		if (input == "仙人指路") input = "兵3进1";
		input = input.Replace("進", "进").Replace("後", "后")
			.Replace("一", "1").Replace("二", "2").Replace("三", "3")
			.Replace("四", "4").Replace("五", "5").Replace("六", "6")
			.Replace("七", "7").Replace("八", "8").Replace("九", "9");
		if (step % 2 > 0) {
			input = input.Replace("車", "俥").Replace("馬", "傌")
				.Replace("车", "俥").Replace("马", "傌").Replace("炮", "砲")
				.Replace("將", "帥").Replace("将", "帥").Replace("帅", "帥")
				.Replace("士", "仕").Replace("象", "相").Replace("卒", "兵");
		} else {
			input = input.Replace("俥", "車").Replace("傌", "馬")
				.Replace("车", "車").Replace("马", "馬").Replace("砲", "炮")
				.Replace("將", "將").Replace("将", "將").Replace("帅", "將")
				.Replace("仕", "士").Replace("相", "象").Replace("兵", "卒");
		}
		str += chess_play(input);
	}
	return str;
};
