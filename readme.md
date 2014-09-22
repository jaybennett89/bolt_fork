# ![Bolt Engine](https://github.com/tonybilby/bolt/blob/master/z-bolt-logo.png "Bolt Engine")
"Best Unity networking solution period."
## Links
Get the latest [Bolt daily build](https://github.com/BoltEngine/bolt/releases).

[Website](http://www.boltengine.com/) | [Forum](http://forum.boltengine.com/) | [Support Chat](https://jabbr.net/#/rooms/bolt)

[Bolt on the Asset Store](https://www.assetstore.unity3d.com/en/#!/content/18358) | [Bolt Unity Forum Thread](http://forum.unity3d.com/threads/released-bolt-the-new-generation-of-networking-solution-for-unity.248912/) | [Bolt Youtube Channel](https://www.youtube.com/channel/UC9NVIbI5rpP7zmEOiB2Cs6Q)

## EULA (End User License Agreement)
THIS IS A LEGAL DOCUMENT. PLEASE READ THIS DOCUMENT CAREFULLY BEFORE USING THIS SOFTWARE. THIS LICENSE PROVIDES IMPORTANT INFORMATION CONCERNING THE SOFTWARE, PROVIDES YOU WITH A LICENSE TO USE THE SOFTWARE AND CONTAINS WARRANTY AND LIABILITY INFORMATION. BY USING THE SOFTWARE, YOU ARE ACCEPTING THE SOFTWARE “AS IS” AND AGREEING TO BE BOUND BY THE TERMS OF THIS LICENSE AGREEMENT. IF YOU DO NOT WISH TO BE LEGALLY BOUND BY THIS AGREEMENT, DO NOT USE THE SOFTWARE.

1. Definitions

(a) The Software - Refers to the Bolt networking engine software product.
(b) The Licensee - Refers to the person or entity which has purchased a license for Bolt.
2. Terms of License

Arpi & Holmström Ek. För. grants to the Licensee a non-exclusive, worldwide, and perpetual license to the Software to integrate the Software only as incorporated and embedded components of electronic games and interactive media and distribute such electronic game and interactive media, except for game services software development kits (“Services SDKs”). The licensee may modify the Software. The licensee may otherwise not sell, reproduce, distribute, sublicense, rent, lease or lend the Software. It is emphasized that the Licensee shall not be entitled to distribute or transfer in any way (including, without, limitation by way of sublicense) the Software. in any other way than as integrated components of electronic games and interactive media. Without limitation of the foregoing it is emphasized that the Licensee shall not be entitled to share the costs related to purchasing the Software. and then let any third party that has contributed to such purchase use the Software.

3. Ownership

This license provides the Licensee with limited rights to use the Software. Arpi & Holmström Ek. För. retains all ownership, right, title and interest in, to and of the Software and all copies of it. All rights not specifically granted in this license, including domestic and international copyrights, are reserved by Arpi & Holmström Ek. För.

4. Proprietary Markings

Arpi & Holmström Ek. För. and Bolt logos, product names, manuals, documentation, and other support materials are either patented, copyrighted, trademarked, constitute valuable trade secrets (whether or not any portion of them may be copyrighted or patented) or are otherwise proprietary to Arpi & Holmström Ek. För. The licensee shall not remove or obscure Arpi & Holmström Ek. För. copyright, trade mark or other proprietary notices from any of the materials contained in this package or downloaded together with the Software.

5. Disclaimer of Warranties and Technical Support

The Software is provided to the Licensee on an “as is” basis, without any technical support or warranty of any kind including, without limitation, any warranty or condition of merchantability, fitness for a particular purpose and non infringement. SOME JURISDICTIONS DO NOT ALLOW THE EXCLUSION OF IMPLIED WARRANTIES, SO THE ABOVE EXCLUSION MAY NOT APPLY TO THE LICENSEE. THE LICENSEE MAY ALSO HAVE OTHER LEGAL RIGHTS WHICH VARY FROM JURISDICTION TO JURISDICTION.

6. Limitation of Liability

ARPI & HOLMSTRÖM EK. FÖR. SHALL NOT BE LIABLE FOR ANY INDIRECT, SPECIAL, INCIDENTAL OR CONSEQUENTIAL DAMAGES OR LOSS, INCLUDING DAMAGES FOR LOSS OF BUSINESS, LOSS OF PROFITS, OR THE LIKE, WHETHER BASED ON BREACH OF CONTRACT, TORT (INCLUDING NEGLIGENCE), PRODUCT LIABILITY OR OTHERWISE, EVEN IF ARPI & HOLMSTRÖM EK. FÖR. OR ITS REPRESENTATIVES HAVE BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGES. SOME JURISDICTIONS DO NOT ALLOW THE LIMITATION OR EXCLUSION OF LIABILITY FOR INCIDENTAL OR CONSEQUENTIAL DAMAGES, SO THIS LIMITATION OR EXCLUSION MAY NOT APPLY TO THE LICENSEE. This warranty gives the Licensee specific legal rights, and the Licensee may also have other rights which vary from jurisdiction to jurisdiction.

7. Term and Termination

This license agreement is effective until terminated. The licensee may terminate this license agreement at any other time by destroying all complete and partial copies of the Software in the Licensees possession. This license and the Licensees rights hereunder shall automatically terminate if the Licensee fail to comply with any provision of this license. Upon such termination, the Licensee shall cease all use of the Software and delete the Software and destroy all copies of the Software and other materials related to the Software in the Licensees possession or under the Licensees control.

8. General Provisions

(a)	This Agreement shall be governed by the laws of Sweden.
(b)	This Agreement contains the complete agreement between the parties with respect to the subject matter hereof, and supersedes all prior or contemporaneous agreements or understandings, whether oral or written.
(c)	All questions, comments or concerns with respect to this Agreement shall be directed to: support@boltengine.com.


## FAQ (Frequently asked questions)

#### Q: What are Bolt's theoretical concurrent user limits
There are no direct limits inside Bolt itself, any of the hard coded "max" values that are defined can be changed by re-compiling your code yourself, it will solely depend on your game and how much data you send over the network.

#### Q: Does Bolt require Unity Pro in any way?
No, the only feature in Bolt which is not available in without Unity Pro is the "Debug Start" feature due to limitations within Unity Free itself.

#### Q: What is a BoltEntity?
A BoltEntity is similar to a Unity/uLink NetworkView or Photon PhotonView. It is the representation of a network-aware object, and is the base for having Bolt control an actual GameObject in Unity.

[Read More FAQ](http://forum.boltengine.com/viewforum.php?f=8)

Upcomming / TODO
-----
- [ ] Adding DNS lookup support
- [ ] Windows Phone 8 support
- [ ] Adding Steam support
