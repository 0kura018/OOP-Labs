# Expected command outcomes

Line numbers include comments and blank lines. Comparisons are INFO entries. Creation/assignment entries are TRACE. State changes still happen when their messages are filtered.

## shop-monday.txt

| Line | Expected action |
|---:|---|
| 1 | Ignore comment. |
| 2 | Create Bob. |
| 3 | Create Alice. |
| 4 | Create Dell-U2723QE, about 27.0 inches, 163 PPI. |
| 5 | Create LG-27GP850, about 27.0 inches, 108.8 PPI. |
| 6 | Create Samsung-Odyssey-G5, about 31.6 inches, 93 PPI. |
| 7 | Create Apple-StudioDisplay, about 26.9 inches, 218 PPI. |
| 8 | Add Dell reference to Bob. |
| 9 | Add LG reference to Bob. |
| 10 | Add Samsung reference to Bob. |
| 11 | Add Apple reference to Alice. |
| 12 | Add the SAME Dell reference to Alice. |
| 13 | Compare Dell/Apple; Dell is slightly larger, Apple is sharper. |
| 14 | Compare Bob's adjacent pairs: Dell/LG, then LG/Samsung. |
| 15 | Remove LG from Bob, return its reference, log purchase. |
| 16 | Compare Dell/Samsung. |
| 17 | Ignore comment. |
| 18 | Remove Dell from Bob, leaving Samsung. |
| 19 | Alice still compares Apple/Dell. Her Dell reference was not removed. |

Expected ERROR count: 0. The Apple diagonal shown here is calculated from the supplied pixel/PPI values; product naming is not used to infer physical size.

## shop-tuesday.txt

| Line | Expected action |
|---:|---|
| 1 | Ignore comment. |
| 2 | Create Carol. |
| 3 | Create BenQ. |
| 4 | ERROR: `abc` is not numeric PPI; AOC is not created. |
| 5 | ERROR: missing PPI; Asus is not created. |
| 6 | WARN: Carol already exists; ignore duplicate. |
| 7 | ERROR: Dave does not exist. |
| 8 | ERROR: Philips-Evnia does not exist. |
| 9 | Assign BenQ to Carol. |
| 10 | WARN: duplicate assignment; keep one reference. |
| 11 | ERROR: Asus does not exist, since line 5 failed. |
| 12 | ERROR: SELL is not a supported command. |
| 13 | ERROR: lowercase `assist` is unknown. A separate correctly spelled `ASSIST carol` would also fail, since names are case-sensitive. |
| 14 | Buy BenQ; Carol becomes empty and produces WARN. |
| 15 | ERROR: BenQ is no longer assigned to Carol (it still exists in the catalog). |
| 16 | WARN: empty Carol cannot compare displays. |
| 17 | ERROR: COMPARE requires two models. |

Expected ERROR count: 9. Errors and warnings do not prevent later lines from executing.

## shop-edge-cases.txt

Expected ERRORs: line 5 (extra manager argument), 6 (zero width), 7 (negative height), 8 (infinite PPI), 9 (NaN PPI), 10 (integer overflow). The invalid records must not be added.

The file also checks blank/indented comments, duplicate model creation, one-display ASSIST, equal displays, a purchase that empties the list, reassignment from the retained catalog and successful manager creation after all errors.
