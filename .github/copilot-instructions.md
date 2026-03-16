# Copilot Instructions

## Project Guidelines
- 回答は日本語(ja-JP)でMarkdown、短く明快に。コードブロックは ```<language> <target file path> 形式で出力する。Visual Studio のコマンド/設定名は __CommandName__ 形式で囲む。
- 不要なnull条件演算子（`?.`）は使わない。例：`JsonUtility.FromJson`の戻り値など、nullにならないことが明らかな場合は`?.`を省略する。
- foreach/if/for などのブロック文では、1行であっても {} を省略しない。